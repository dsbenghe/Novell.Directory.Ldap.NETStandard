/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The class that creates a connection to the Ldap server. After the
    ///     connection is made, a thread is created that reads data from the
    ///     connection.
    ///     The application's thread sends a request to the MessageAgent class, which
    ///     creates a Message class.  The Message class calls the writeMessage method
    ///     of this class to send the request to the server. The application thread
    ///     will then query the MessageAgent class for a response.
    ///     The reader thread multiplexes response messages received from the
    ///     server to the appropriate Message class. Each Message class
    ///     has its own message queue.
    ///     Unsolicited messages are process separately, and if the application
    ///     has registered a handler, a separate thread is created for that
    ///     application's handler to process the message.
    ///     Note: the reader thread must not be a "selfish" thread, since some
    ///     operating systems do not time slice.
    /// </summary>
    /*package*/
#pragma warning disable CA1001 // Types that own disposable fields should be disposable - Disposed via DestroyClone
    internal class Connection : IDebugIdentifier
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
    {
        private readonly LdapConnectionOptions _ldapConnectionOptions;

        // Ldap message IDs are all positive numbers so we can use negative
        //  numbers as flags.  This are flags assigned to stopReaderMessageID
        //  to tell the reader what state we are in.
        private const int ContinueReading = -99;

        private const int StopReading = -98;

        // These attributes can be retreived using the getProperty
        // method in LdapConnection.  Future releases might require
        // these to be local variables that can be modified using
        // the setProperty method.
        internal const string Sdk = "2.2.1";

        internal const int Protocol = 3;

        internal const string Security = "simple";

        private readonly object _lock = new object();

        // When set to true the client connection is up and running
        private bool _clientActive = true;

        // The LdapSocketFactory to be used as the default to create new connections
        // private static LdapSocketFactory socketFactory = null;
        // The LdapSocketFactory used for this connection
        // private LdapSocketFactory mySocketFactory;
        // Number of clones in addition to original LdapConnection using this
        // connection.
        private int _cloneCount;
        private Thread _deadReader; // Identity of last reader thread
        private Exception _deadReaderException; // Last exception of reader
        private readonly LberDecoder _decoder;

        private readonly LberEncoder _encoder;

        // We need a message number for disconnect to grab the semaphore,
        // but may not have one, so we invent a unique one.
        private int _ephemeralId = -1;
        private X509ChainStatus[] _handshakeChainStatus;
        private SslPolicyErrors _handshakePolicyErrors;

        private Stream _inStream;

        // Place to save message information classes
        private readonly MessageVector _messages;
        private TcpClient _nonTlsBackup;
        private Stream _outStream;
        private Thread _reader; // New thread that reads data from the server.

        private ReaderThread _readerThreadEnclosure;

        /*
        * socket is the current socket being used.
        * nonTLSBackup is the backup socket if startTLS is called.
        * if nonTLSBackup is null then startTLS has not been called,
        * or stopTLS has been called to end TLS protection
        */
        private Socket _sock;
        private TcpClient _socket;

        // Stops the reader thread when a Message with the passed-in ID is read.
        //  This parameter is set by stopReaderOnReply and stopTLS
        private int _stopReaderMessageId;

        // Connection created to follow referral

        // Place to save unsolicited message listeners
        private readonly IList<ILdapUnsolicitedNotificationListener> _unsolicitedListeners;

        // Indicates we have received a server shutdown unsolicited notification
        private bool _unsolSvrShutDnNotification;

        private readonly object _writeSemaphore = new object();
        private int _writeSemaphoreCount;
        private int _writeSemaphoreOwner;

        public virtual DebugId DebugId { get; } = DebugId.ForType<Connection>();

        /// <summary>
        ///     Create a new Connection object.
        /// </summary>
        internal Connection(LdapConnectionOptions ldapConnectionOptions)
        {
            _ldapConnectionOptions = ldapConnectionOptions;
            _encoder = new LberEncoder();
            _decoder = new LberDecoder();
            _stopReaderMessageId = ContinueReading;
            _messages = new MessageVector(5);
            _unsolicitedListeners = new List<ILdapUnsolicitedNotificationListener>(3);
        }

        /// <summary>
        ///     Indicates whether clones exist for LdapConnection.
        /// </summary>
        /// <returns>
        ///     true if clones exist, false otherwise.
        /// </returns>
        internal bool Cloned => _cloneCount > 0;

        /// <summary> gets the host used for this connection.</summary>
        internal string Host { get; private set; }

        /// <summary> gets the port used for this connection.</summary>
        internal int Port { get; private set; }

        internal int ConnectionTimeout { get; set; }

        /// <summary> gets the writeSemaphore id used for active bind operation.</summary>
        internal int BindSemId { get; private set; }

        /// <summary> checks if the writeSemaphore id used for active bind operation is clear.</summary>
        internal bool BindSemIdClear => BindSemId == 0;

        /// <summary>
        ///     Return whether the application is bound to this connection.
        ///     Note: an anonymous bind returns false - not bound.
        /// </summary>
        internal bool Bound
        {
            get
            {
                if (BindProperties != null)
                {
                    // Bound if not anonymous
                    return !BindProperties.Anonymous;
                }

                return false;
            }
        }

        /// <summary> Return whether a connection has been made.</summary>
        internal bool Connected => _inStream != null;

        /// <summary>
        ///     Sets the authentication credentials in the object
        ///     and set flag indicating successful bind.
        /// </summary>
        /// <returns>
        ///     The BindProperties object for this connection.
        /// </returns>
        internal BindProperties BindProperties { get; set; }

        /// <summary>
        ///     Gets the current referral active on this connection if created to
        ///     follow referrals.
        /// </summary>
        /// <returns>
        ///     the active referral url.
        /// </returns>
        /// <summary>
        ///     Sets the current referral active on this connection if created to
        ///     follow referrals.
        /// </summary>
        internal ReferralInfo ActiveReferral { get; set; }

        /// <summary>
        ///     Indicates if the connection is using TLS protection
        ///     Return true if using TLS protection.
        /// </summary>
        internal bool Tls => _nonTlsBackup != null;

        internal System.Net.Security.RemoteCertificateValidationCallback OnRemoteCertificateValidation { get; set; }

        internal System.Net.Security.LocalCertificateSelectionCallback OnLocalCertificateSelection { get; set; }

        private string GetSslHandshakeErrors()
        {
            var strMsg = "Following problem(s) occurred while establishing SSL based Connection : ";
            if (_handshakePolicyErrors != SslPolicyErrors.None)
            {
                strMsg += _handshakePolicyErrors;
                foreach (var chainStatus in _handshakeChainStatus)
                {
                    if (chainStatus.Status != X509ChainStatusFlags.NoError)
                    {
                        strMsg += ", " + chainStatus.StatusInformation;
                    }
                }
            }
            else
            {
                strMsg += "Unknown Certificate Problem";
            }

            return strMsg;
        }

        /// <summary>
        ///     Copy this Connection object.
        ///     This is not a true clone, but creates a new object encapsulating
        ///     part of the connection information from the original object.
        ///     The new object will have the same default socket factory,
        ///     designated socket factory, host, port, and protocol version
        ///     as the original object.
        ///     The new object is NOT be connected to the host.
        /// </summary>
        /// <returns>
        ///     a shallow copy of this object.
        /// </returns>
        private object Copy()
        {
            var c = new Connection(_ldapConnectionOptions)
            {
                Host = Host,
                Port = Port,
            };
            return c;
        }

        /// <summary>
        ///     Acquire a simple counting semaphore that synchronizes state affecting
        ///     bind. This method generates an ephemeral message id (negative number).
        ///     We bind using the message ID because a different thread may unlock
        ///     the semaphore than the one that set it.  It is cleared when the
        ///     response to the bind is processed, or when the bind operation times out.
        ///     Returns when the semaphore is acquired.
        /// </summary>
        /// <returns>
        ///     the ephemeral message id that identifies semaphore's owner.
        /// </returns>
        internal int AcquireWriteSemaphore()
        {
            return AcquireWriteSemaphore(0);
        }

        /// <summary>
        ///     Acquire a simple counting semaphore that synchronizes state affecting
        ///     bind. The semaphore is held by setting a value in writeSemaphoreOwner.
        ///     We bind using the message ID because a different thread may unlock
        ///     the semaphore than the one that set it.  It is cleared when the
        ///     response to the bind is processed, or when the bind operation times out.
        ///     Returns when the semaphore is acquired.
        /// </summary>
        /// <param name="msgId">
        ///     a value that identifies the owner of this semaphore. A
        ///     value of zero means assign a unique semaphore value.
        /// </param>
        /// <returns>
        ///     the semaphore value used to acquire the lock.
        /// </returns>
        internal int AcquireWriteSemaphore(int msgId)
        {
            var id = msgId;
            lock (_writeSemaphore)
            {
                if (id == 0)
                {
                    _ephemeralId = _ephemeralId == int.MinValue ? (_ephemeralId = -1) : --_ephemeralId;
                    id = _ephemeralId;
                }

                while (true)
                {
                    if (_writeSemaphoreOwner == 0)
                    {
                        // we have acquired the semaphore
                        _writeSemaphoreOwner = id;
                        break;
                    }

                    if (_writeSemaphoreOwner == id)
                    {
                        // we already own the semaphore
                        break;
                    }

                    // Keep trying for the lock
                    Monitor.Wait(_writeSemaphore);
                }

                _writeSemaphoreCount++;
            }

            return id;
        }

        /// <summary>
        ///     Release a simple counting semaphore that synchronizes state affecting
        ///     bind.  Frees the semaphore when number of acquires and frees for this
        ///     thread match.
        /// </summary>
        /// <param name="msgId">
        ///     a value that identifies the owner of this semaphore.
        /// </param>
        internal void FreeWriteSemaphore(int msgId)
        {
            lock (_writeSemaphore)
            {
                if (_writeSemaphoreOwner == 0)
                {
                    throw new Exception("Connection.freeWriteSemaphore(" + msgId +
                                        "): semaphore not owned by any thread");
                }

                if (_writeSemaphoreOwner != msgId)
                {
                    throw new Exception("Connection.freeWriteSemaphore(" + msgId +
                                        "): thread does not own the semaphore, owned by " + _writeSemaphoreOwner);
                }

                // if all instances of this semaphore for this thread are released,
                // wake up all threads waiting.
                if (--_writeSemaphoreCount == 0)
                {
                    _writeSemaphoreOwner = 0;
                    Monitor.Pulse(_writeSemaphore);
                }
            }
        }

        /*
        * Wait until the reader thread ID matches the specified parameter.
        * Null = wait for the reader to terminate
        * Non Null = wait for the reader to start
        * Returns when the ID matches, i.e. reader stopped, or reader started.
        *
        * @param the thread id to match
        */

        private void WaitForReader(Thread thread)
        {
            // wait for previous reader thread to terminate
            var rInst = _reader;
            var tInst = thread;
            while (!Equals(rInst, tInst))
            {
                // Don't initialize connection while previous reader thread still
                // active.
                /*
                * The reader thread may start and immediately terminate.
                * To prevent the waitForReader from waiting forever
                * for the dead to rise, we leave traces of the deceased.
                * If the thread is already gone, we throw an exception.
                */
                if (thread == _deadReader)
                {
                    /* then we wanted a shutdown */
                    if (thread == null)
                    {
                        return;
                    }

                    var readerException = _deadReaderException;
                    _deadReaderException = null;
                    _deadReader = null;

                    // Reader thread terminated
                    throw new LdapException(ExceptionMessages.ConnectionReader, LdapException.ConnectError, null,
                        readerException);
                }

                lock (_lock)
                {
                    Monitor.Wait(_lock, TimeSpan.FromMilliseconds(5));
                }

                rInst = _reader;
                tInst = thread;
            }

            _deadReaderException = null;
            _deadReader = null;
        }

        /// <summary>
        ///     Constructs a TCP/IP connection to a server specified in host and port.
        /// </summary>
        /// <param name="host">
        ///     The host to connect to.
        /// </param>
        /// <param name="port">
        ///     The port on the host to connect to.
        /// </param>
        internal Task ConnectAsync(string host, int port, CancellationToken ct = default)
        {
            return ConnectAsync(host, port, 0, ct);
        }

        /****************************************************************************/

        internal bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (_ldapConnectionOptions.RemoteCertificateValidationCallback != null)
            {
                return _ldapConnectionOptions.RemoteCertificateValidationCallback(
                    sender, certificate, chain, sslPolicyErrors);
            }

            if (OnRemoteCertificateValidation != null)
            {
                return OnRemoteCertificateValidation(sender, certificate, chain, sslPolicyErrors);
            }

            return DefaultCertificateValidationHandler(certificate, chain, sslPolicyErrors);
        }

        internal X509Certificate LocalCertificateSelectionCallback(object sender, string targetHost,
            X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            if (_ldapConnectionOptions.LocalCertificateSelectionCallback != null)
            {
                return _ldapConnectionOptions.LocalCertificateSelectionCallback(
                    sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
            }

            return OnLocalCertificateSelection?.Invoke(
                sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
        }

        private bool DefaultCertificateValidationHandler(
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            _handshakeChainStatus = chain.ChainStatus;
            _handshakePolicyErrors = sslPolicyErrors;
            return false;
        }

        /***********************************************************************/

        /// <summary>
        ///     Constructs a TCP/IP connection to a server specified in host and port.
        ///     Starts the reader thread.
        /// </summary>
        /// <param name="host">
        ///     The host to connect to.
        /// </param>
        /// <param name="port">
        ///     The port on the host to connect to.
        /// </param>
        /// <param name="semaphoreId">
        ///     The write semaphore ID to use for the connect.
        /// </param>
        private async Task ConnectAsync(string host, int port, int semaphoreId, CancellationToken ct = default)
        {
            /* Synchronized so all variables are in a consistant state and
            * so that another thread isn't doing a connect, disconnect, or clone
            * at the same time.
            */
            // Wait for active reader to terminate
            WaitForReader(null);

            // Clear the server shutdown notification flag.  This should already
            // be false unless of course we are reusing the same Connection object
            // after a server shutdown notification
            _unsolSvrShutDnNotification = false;

            var semId = AcquireWriteSemaphore(semaphoreId);
            try
            {
                // Make socket connection to specified host and port
                if (port == 0)
                {
                    port = LdapConnection.DefaultPort;
                }

                try
                {
                    if (_inStream == null || _outStream == null)
                    {
                        Host = host;
                        Port = port;

                        if (!IPAddress.TryParse(host, out var ipAddress))
                        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
                            var ipAddresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
#else
                            var ipAddresses = await Dns.GetHostAddressesAsync(host, ct).ConfigureAwait(false);
#endif
                            ipAddress = ipAddresses
                                .Where(x => _ldapConnectionOptions.IpAddressFilter(x))
                                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork
                                             || ip.AddressFamily == AddressFamily.InterNetworkV6);

                            if (ipAddress == null)
                            {
                                throw new ArgumentException("No ip address found", nameof(ipAddress));
                            }
                        }

                        if (_ldapConnectionOptions.Ssl)
                        {
                            _sock = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.IP);
                            var ipEndPoint = new IPEndPoint(ipAddress, port);
#if NETSTANDARD2_0 || NETSTANDARD2_1
                            await _sock.ConnectAsync(ipEndPoint).TimeoutAfterAsync(ConnectionTimeout).ConfigureAwait(false);
#else
                            await _sock.ConnectAsync(ipEndPoint, ct).TimeoutAfterAsync(ConnectionTimeout).ConfigureAwait(false);
#endif

                            var sslStream = new SslStream(
                                new NetworkStream(_sock, true),
                                false,
                                RemoteCertificateValidationCallback,
                                LocalCertificateSelectionCallback);
#if NETSTANDARD2_0
                            await sslStream.AuthenticateAsClientAsync(
                                    host,
                                    new X509CertificateCollection(_ldapConnectionOptions.ClientCertificates.ToArray()),
                                    _ldapConnectionOptions.SslProtocols,
                                    _ldapConnectionOptions.CheckCertificateRevocationEnabled)
#else
                            await sslStream.AuthenticateAsClientAsync(
                                    new SslClientAuthenticationOptions()
                                    {
                                        TargetHost = host,
                                        ClientCertificates = new X509CertificateCollection(_ldapConnectionOptions.ClientCertificates.ToArray()),
                                        EnabledSslProtocols = _ldapConnectionOptions.SslProtocols,
                                        CertificateRevocationCheckMode = _ldapConnectionOptions.CheckCertificateRevocationEnabled ?
                                            X509RevocationMode.Online :
                                            X509RevocationMode.NoCheck,
                                    },
                                    ct)
#endif
                                .TimeoutAfterAsync(ConnectionTimeout)
                                .ConfigureAwait(false);

                            _inStream = sslStream;
                            _outStream = sslStream;
                        }
                        else
                        {
                            _socket = new TcpClient(ipAddress.AddressFamily);
#if NETSTANDARD2_0 || NETSTANDARD2_1
                            await _socket.ConnectAsync(host, port).TimeoutAfterAsync(ConnectionTimeout).ConfigureAwait(false);
#else
                            await _socket.ConnectAsync(host, port, ct).TimeoutAfterAsync(ConnectionTimeout).ConfigureAwait(false);
#endif

                            _inStream = _socket.GetStream();
                            _outStream = _socket.GetStream();
                        }
                    }
                    else
                    {
                        Console.WriteLine("connect input/out Stream specified");
                    }
                }
                catch (SocketException se)
                {
                    _sock = null;
                    _socket = null;
                    throw new LdapException(ExceptionMessages.ConnectionError, new object[] { host, port },
                        LdapException.ConnectError, null, se);
                }
                catch (IOException ioe)
                {
                    _sock = null;
                    _socket = null;
                    throw new LdapException(ExceptionMessages.ConnectionError, new object[] { host, port },
                        LdapException.ConnectError, null, ioe);
                }

                // Set host and port
                Host = host;
                Port = port;

                // start the reader thread
                StartReader();
                _clientActive = true; // Client is up
            }
            finally
            {
                FreeWriteSemaphore(semId);
            }
        }

        /// <summary>  Increments the count of cloned connections.</summary>
        internal void IncrCloneCount()
        {
            lock (_lock)
            {
                _cloneCount++;
            }
        }

        /// <summary>
        ///     Destroys a clone of. <code>LdapConnection</code>.
        ///     This method first determines if only one. <code>LdapConnection</code>
        ///     object is associated with this connection, i.e. if no clone exists.
        ///     If no clone exists, the socket is closed, and the current.
        ///     <code>Connection</code> object is returned.
        ///     If multiple. <code>LdapConnection</code> objects are associated
        ///     with this connection, i.e. clones exist, a {@link #copy} of the
        ///     this object is made, but is not connected to any host. This
        ///     disassociates that clone from the original connection.  The new.
        ///     <code>Connection</code> object is returned.
        ///     Only one destroyClone instance is allowed to run at any one time.
        ///     If the connection is closed, any threads waiting for operations
        ///     on that connection will wake with an LdapException indicating
        ///     the connection is closed.
        /// </summary>
        /// <returns>
        ///     a Connection object or null if finalizing.
        /// </returns>
        internal Connection DestroyClone()
        {
            lock (_lock)
            {
                var conn = this;

                if (_cloneCount > 0)
                {
                    _cloneCount--;
                    conn = (Connection)Copy();
                }
                else
                {
                    if (_inStream != null)
                    {
                        // Not a clone and connected
                        /*
                        * Either the application has called disconnect or connect
                        * resulting in the current connection being closed. If the
                        * application has any queues waiting on messages, we
                        * need wake these up so the application does not hang.
                        * The boolean flag indicates whether the close came
                        * from an API call or from the object being finalized.
                        */
                        var notify = new InterThreadException(ExceptionMessages.ConnectionClosed, null,
                            LdapException.ConnectError, null, null);

                        // Destroy old connection
                        Destroy("destroy clone", 0, notify);
                    }
                }

                return conn;
            }
        }

        /// <summary> clears the writeSemaphore id used for active bind operation.</summary>
        internal void ClearBindSemId()
        {
            BindSemId = 0;
        }

        internal void SetBindSemId(int bindSemId)
        {
            if (!BindSemIdClear)
            {
                throw new InvalidOperationException($"There is already a Bind Semaphore ID set ({BindSemId}), setting it to {bindSemId} not possible.");
            }

            BindSemId = bindSemId;
        }

        /// <summary>
        ///     Writes an LdapMessage to the Ldap server over a socket.
        /// </summary>
        /// <param name="info">
        ///     the Message containing the message to write.
        /// </param>
        internal async Task WriteMessageAsync(Message info, CancellationToken ct = default)
        {
            _messages.Add(info);

            // For bind requests, if not connected, attempt to reconnect
            if (info.BindRequest && Connected == false && Host != null)
            {
                await ConnectAsync(Host, Port, info.MessageId, ct).ConfigureAwait(false);
            }

            if (Connected)
            {
                var msg = info.Request;
                WriteMessage(msg);
            }
            else
            {
                throw new LdapException(ExceptionMessages.ConnectionClosed, new object[] { Host, Port },
                    LdapException.ConnectError, null);
            }
        }

        /// <summary>
        ///     Writes an LdapMessage to the Ldap server over a socket.
        /// </summary>
        /// <param name="msg">
        ///     the message to write.
        /// </param>
        internal void WriteMessage(LdapMessage msg)
        {
            int id;

            // Get the correct semaphore id for bind operations
            if (BindSemId == 0)
            {
                // Semaphore id for normal operations
                id = msg.MessageId;
            }
            else
            {
                // Semaphore id for sasl bind operations
                id = BindSemId;
            }

            var myOut = _outStream;

            AcquireWriteSemaphore(id);
            try
            {
                if (myOut == null)
                {
                    throw new IOException("Output stream not initialized");
                }

                if (!myOut.CanWrite)
                {
                    return;
                }

                var ber = msg.Asn1Object.GetEncoding(_encoder);
                myOut.Write(ber, 0, ber.Length);
                myOut.Flush();
            }
            catch (IOException ioe)
            {
                if (msg.Type == LdapMessage.BindRequest && _ldapConnectionOptions.Ssl)
                {
                    var strMsg = GetSslHandshakeErrors();
                    throw new LdapException(strMsg, new object[] { Host, Port }, LdapException.SslHandshakeFailed, null,
                        ioe);
                }

                /*
                * IOException could be due to a server shutdown notification which
                * caused our Connection to quit.  If so we send back a slightly
                * different error message.  We could have checked this a little
                * earlier in the method but that would be an expensive check each
                * time we send out a message.  Since this shutdown request is
                * going to be an infrequent occurence we check for it only when
                * we get an IOException.  shutdown() will do the cleanup.
                */
                if (_clientActive)
                {
                    // We beliefe the connection was alive
                    if (_unsolSvrShutDnNotification)
                    {
                        // got server shutdown
                        throw new LdapException(ExceptionMessages.ServerShutdownReq, new object[] { Host, Port },
                            LdapException.ConnectError, null, ioe);
                    }

                    // Other I/O Exceptions on host:port are reported as is
                    throw new LdapException(ExceptionMessages.IoException, new object[] { Host, Port },
                        LdapException.ConnectError, null, ioe);
                }
            }
            finally
            {
                FreeWriteSemaphore(id);
                _handshakePolicyErrors = SslPolicyErrors.None;
            }
        }

        /// <summary> Returns the message agent for this msg ID.</summary>
        internal MessageAgent GetMessageAgent(int msgId)
        {
            var info = _messages.FindMessageById(msgId);
            return info.MessageAgent;
        }

        /// <summary>
        ///     Removes a Message class from the Connection's list.
        /// </summary>
        /// <param name="info">
        ///     the Message class to remove from the list.
        /// </param>
        internal void RemoveMessage(Message info)
        {
            _messages.Remove(info);
        }

        private void Destroy(string reason, int semaphoreId, InterThreadException notifyUser)
        {
            if (!_clientActive)
            {
                return;
            }

            _clientActive = false;
            AbandonMessages(notifyUser);

            var semId = AcquireWriteSemaphore(semaphoreId);
            try
            {
                // Now send unbind if socket not closed
                if (BindProperties is { Anonymous: false } && _outStream is { CanWrite: true })
                {
                    try
                    {
                        var msg = new LdapUnbindRequest(null);
                        var ber = msg.Asn1Object.GetEncoding(_encoder);
                        _outStream.Write(ber, 0, ber.Length);
                        _outStream.Flush();
                    }
                    catch (Exception)
                    {
                        // don't worry about error
                    }
                }

                // If (still) using Tls then stop. Otherwise it takes 15 minutes to finish
                if (Tls)
                {
                    try
                    {
                        StopTls();
                    }
                    catch (Exception)
                    {
                        // don't worry about error
                    }
                }

                BindProperties = null;
                if (_socket != null || _sock != null)
                {
                    // Just before closing the sockets, abort the reader thread
                    if (_reader != null && reason != "reader: thread stopping")
                    {
                        _readerThreadEnclosure.Stop();
                    }

                    // Close the socket
                    try
                    {
                        _inStream?.Dispose();
                        _outStream?.Dispose();
                        _sock?.Dispose();
                        _socket?.Dispose();
                    }
                    catch (IOException)
                    {
                        // ignore problem closing socket
                    }

                    _socket = null;
                    _sock = null;
                    _inStream = null;
                    _outStream = null;
                }
            }
            finally
            {
                FreeWriteSemaphore(semId);
            }
        }

        private void AbandonMessages(InterThreadException notifyUser)
        {
            // remove messages from connection list and send abandon
            var leftMessages = _messages.RemoveAll();
            foreach (Message message in leftMessages)
            {
                message.Abandon(null, notifyUser); // also notifies the application
            }
        }

        /// <summary>
        ///     This tests to see if there are any outstanding messages.  If no messages
        ///     are in the queue it returns true.  Each message will be tested to
        ///     verify that it is complete.
        ///     <I>The writeSemaphore must be set for this method to be reliable!</I>
        /// </summary>
        /// <returns>
        ///     true if no outstanding messages.
        /// </returns>
        internal bool AreMessagesComplete()
        {
            var leftMessages = _messages.RemoveAll();
            var length = leftMessages.Length;

            // Check if SASL bind in progress
            if (BindSemId != 0)
            {
                return false;
            }

            // Check if any messages queued
            if (length == 0)
            {
                return true;
            }

            for (var i = 0; i < length; i++)
            {
                if (((Message)leftMessages[i]).Complete == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     The reader thread will stop when a reply is read with an ID equal
        ///     to the messageID passed in to this method.  This is used by
        ///     LdapConnection.StartTLS.
        /// </summary>
        internal void StopReaderOnReply(int messageId)
        {
            _stopReaderMessageId = messageId;
        }

        /// <summary>
        ///     startReader
        ///     startReader should be called when socket and io streams have been
        ///     set or changed.  In particular after client.Connection.startTLS()
        ///     It assumes the reader thread is not running.
        /// </summary>
        private void StartReader()
        {
            // Start Reader Thread
            var r = new Thread(new ReaderThread(this).Run)
            {
                IsBackground = true, // If the last thread running, allow exit.
            };
            r.Start();
            WaitForReader(r);
        }

        /// <summary>
        ///     StartsTLS, in this package, assumes the caller has:
        ///     1) Acquired the writeSemaphore
        ///     2) Stopped the reader thread
        ///     3) checked that no messages are outstanding on this connection.
        ///     After calling this method upper layers should start the reader
        ///     by calling startReader()
        ///     In the client.Connection, StartTLS assumes Ldap.LdapConnection will
        ///     stop and start the reader thread.  Connection.StopTLS will stop
        ///     and start the reader thread.
        /// </summary>
        internal async Task StartTlsAsync(CancellationToken ct = default)
        {
            try
            {
                WaitForReader(null);
                _nonTlsBackup = _socket;
                var sslStream = new SslStream(
                    _socket.GetStream(),
                    true,
                    RemoteCertificateValidationCallback,
                    LocalCertificateSelectionCallback);
                #if NETSTANDARD2_0
                await sslStream.AuthenticateAsClientAsync(
                        Host,
                        new X509CertificateCollection(_ldapConnectionOptions.ClientCertificates.ToArray()),
                        _ldapConnectionOptions.SslProtocols,
                        _ldapConnectionOptions.CheckCertificateRevocationEnabled)
                #else
                await sslStream.AuthenticateAsClientAsync(
                        new SslClientAuthenticationOptions()
                        {
                            TargetHost = Host,
                            ClientCertificates = new X509CertificateCollection(_ldapConnectionOptions.ClientCertificates.ToArray()),
                            EnabledSslProtocols = _ldapConnectionOptions.SslProtocols,
                            CertificateRevocationCheckMode = _ldapConnectionOptions.CheckCertificateRevocationEnabled ?
                                X509RevocationMode.Online :
                                X509RevocationMode.NoCheck,
                        },
                        ct)
                #endif
                    .TimeoutAfterAsync(ConnectionTimeout)
                    .ConfigureAwait(false);
                _inStream = sslStream;
                _outStream = sslStream;
                StartReader();
            }
            catch (Exception ex)
            {
                _nonTlsBackup = null;
                throw new LdapException("Error starting TLS", LdapException.ConnectError, null, ex);
            }
        }

        /*
        * Stops TLS.
        *
        * StopTLS, in this package, assumes the caller has:
        *  1) blocked writing (acquireWriteSemaphore).
        *  2) checked that no messages are outstanding.
        *
        *  StopTLS Needs to do the following:
        *  1) close the current socket
        *      - This stops the reader thread
        *      - set STOP_READING flag on stopReaderMessageID so that
        *        the reader knows that the IOException is planned.
        *  2) replace the current socket with nonTLSBackup,
        *  3) and set nonTLSBackup to null;
        *  4) reset input and outputstreams
        *  5) start the reader thread by calling startReader
        *
        *  Note: Sun's JSSE doesn't allow the nonTLSBackup socket to be
        * used any more, even though autoclose was false: you get an IOException.
        * IBM's JSSE hangs when you close the JSSE socket.
        */

        internal void StopTls()
        {
            try
            {
                _stopReaderMessageId = StopReading;
                _outStream?.Dispose();
                _inStream?.Dispose();

                // this.sock.Shutdown(SocketShutdown.Both);
                // this.sock.Close();
                WaitForReader(null);
                _socket = _nonTlsBackup;
                _inStream = _socket.GetStream();
                _outStream = _socket.GetStream();

                // Allow the new reader to start
                _stopReaderMessageId = ContinueReading;
            }
            catch (IOException ioe)
            {
                throw new LdapException(ExceptionMessages.StoptlsError, LdapException.ConnectError, null, ioe);
            }
            finally
            {
                _nonTlsBackup = null;
            }
        }

        /// <summary>
        ///     Add the specific object to the list of listeners that want to be
        ///     notified when an unsolicited notification is received.
        /// </summary>
        internal void AddUnsolicitedNotificationListener(ILdapUnsolicitedNotificationListener listener)
        {
            _unsolicitedListeners.Add(listener);
        }

        /// <summary>Remove the specific object from current list of listeners.</summary>
        internal void RemoveUnsolicitedNotificationListener(ILdapUnsolicitedNotificationListener listener)
        {
            _unsolicitedListeners.Remove(listener);
        }

        private void NotifyAllUnsolicitedListeners(RfcLdapMessage message)
        {
            // MISSING:  If this is a shutdown notification from the server
            // set a flag in the Connection class so that we can throw an
            // appropriate LdapException to the application
            LdapMessage extendedLdapMessage = new LdapExtendedResponse(message);
            var notificationOid = ((LdapExtendedResponse)extendedLdapMessage).Id;
            if (notificationOid.Equals(LdapConnection.ServerShutdownOid))
            {
                _unsolSvrShutDnNotification = true;
            }

            var numOfListeners = _unsolicitedListeners.Count;

            // Cycle through all the listeners
            for (var i = 0; i < numOfListeners; i++)
            {
                // Get next listener
                var listener = _unsolicitedListeners[i];

                // Create a new ExtendedResponse each time as we do not want each listener
                // to have its own copy of the message
                var tempLdapMessage = new LdapExtendedResponse(message);

                // Spawn a new thread for each listener to go process the message
                // The reason we create a new thread rather than just call the
                // the messageReceived method directly is beacuse we do not know
                // what kind of processing the notification listener class will
                // do.  We do not want our deamon thread to block waiting for
                // the notification listener method to return.
                var u = new UnsolicitedListenerThread(listener, tempLdapMessage);
                u.Start();
            }
        }

        public class ReaderThread
        {
            private readonly Connection _enclosingInstance;
            private Thread _enclosedThread;
            private bool _isStopping;

            public ReaderThread(Connection enclosingInstance)
            {
                _enclosingInstance = enclosingInstance;
            }

            public void Stop()
            {
                if (_enclosedThread == null)
                {
                    return;
                }

                _isStopping = true;

                // This is quite silly as we want to stop the thread gracefully but is not always possible as the Read on socket is blocking
                // Using ReadAsync will not do any good as the method taking the CancellationToken as parameter is not implemented
                // Dispose will break forcefully the Read.
                // We could use a ReadTimeout for socket - but this will only make stopping the thread take longer
                // And we don't care if we just kill the socket stream as we don't plan to reuse the stream after stop
                // the stream Dispose used to be called from Connection dispose but only when a Bind is successful which was causing
                // the Dispose to hang un unsuccessful bind
                // So, yeah isStopping flag is pretty much useless as there are very small chances that it will be hit
                var socketStream = _enclosingInstance._inStream;
                socketStream?.Dispose();

                _enclosedThread.Join();
            }

            /// <summary>
            ///     This thread decodes and processes RfcLdapMessage's from the server.
            ///     Note: This thread needs a graceful shutdown implementation.
            /// </summary>
            public void Run()
            {
                var reason = "reader: thread stopping";
                InterThreadException notify = null;
                Message info = null;
                Exception readerException = null;
                _enclosingInstance._readerThreadEnclosure = this;
                _enclosingInstance._reader = _enclosedThread = Thread.CurrentThread;
                try
                {
                    while (!_isStopping)
                    {
                        // -------------------------------------------------------
                        // Decode an RfcLdapMessage directly from the socket.
                        // -------------------------------------------------------
                        /* get current value of in, keep value consistant
                        * though the loop, i.e. even during shutdown
                        */
                        var myIn = _enclosingInstance._inStream;
                        if (myIn == null)
                        {
                            break;
                        }

                        var asn1Id = new Asn1Identifier(myIn);
                        var tag = asn1Id.Tag;
                        if (asn1Id.Tag != Asn1Sequence.Tag)
                        {
                            continue; // loop looking for an RfcLdapMessage identifier
                        }

                        // Turn the message into an RfcMessage class
                        var asn1Len = new Asn1Length(myIn);

                        var msg = new RfcLdapMessage(_enclosingInstance._decoder, myIn, asn1Len.Length);

                        // ------------------------------------------------------------
                        // Process the decoded RfcLdapMessage.
                        // ------------------------------------------------------------
                        var msgId = msg.MessageId;

                        // Find the message which requested this response.
                        // It is possible to receive a response for a request which
                        // has been abandoned. If abandoned, throw it away
                        try
                        {
                            info = _enclosingInstance._messages.FindMessageById(msgId);
                            info.PutReply(msg); // queue & wake up waiting thread
                        }
                        catch (FieldAccessException)
                        {
                            /*
                            * We get the NoSuchFieldException when we could not find
                            * a matching message id.  First check to see if this is
                            * an unsolicited notification (msgID == 0). If it is not
                            * we throw it away. If it is we call any unsolicited
                            * listeners that might have been registered to listen for these
                            * messages.
                            */

                            /* Note the location of this code.  We could have required
                            * that message ID 0 be just like other message ID's but
                            * since message ID 0 has to be treated specially we have
                            * a separate check for message ID 0.  Also note that
                            * this test is after the regular message list has been
                            * checked for.  We could have always checked the list
                            * of messages after checking if this is an unsolicited
                            * notification but that would have inefficient as
                            * message ID 0 is a rare event (as of this time).
                            */
                            if (msgId == 0)
                            {
                                // Notify any listeners that might have been registered
                                _enclosingInstance.NotifyAllUnsolicitedListeners(msg);

                                /*
                                * Was this a server shutdown unsolicited notification.
                                * IF so we quit. Actually calling the return will
                                * first transfer control to the finally clause which
                                * will do the necessary clean up.
                                */
                                if (_enclosingInstance._unsolSvrShutDnNotification)
                                {
                                    notify = new InterThreadException(
                                        ExceptionMessages.ServerShutdownReq,
                                        new object[] { _enclosingInstance.Host, _enclosingInstance.Port },
                                        LdapException.ConnectError, null, null);

                                    return;
                                }
                            }
                        }

                        if (_enclosingInstance._stopReaderMessageId == msgId ||
                            _enclosingInstance._stopReaderMessageId == StopReading)
                        {
                            // Stop the reader Thread.
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    readerException = ex;
                    if (_enclosingInstance._stopReaderMessageId != StopReading && _enclosingInstance._clientActive)
                    {
                        // Connection lost waiting for results from host:port
                        notify = new InterThreadException(
                            ExceptionMessages.ConnectionWait,
                            new object[] { _enclosingInstance.Host, _enclosingInstance.Port }, LdapException.ConnectError,
                            ex, info);
                    }

                    // The connection is no good, don't use it any more
                    _enclosingInstance._inStream = null;
                    _enclosingInstance._outStream = null;
                }
                finally
                {
                    /*
                    * There can be four states that the reader can be in at this point:
                    *  1) We are starting TLS and will be restarting the reader
                    *     after we have negotiated TLS.
                    *      - Indicated by whether stopReaderMessageID does not
                    *        equal CONTINUE_READING.
                    *      - Don't call Shutdown.
                    *  2) We are stoping TLS and will be restarting after TLS is
                    *     stopped.
                    *      - Indicated by an IOException AND stopReaderMessageID equals
                    *        STOP_READING - in which case notify will be null.
                    *      - Don't call Shutdown
                    *  3) We receive a Server Shutdown notification.
                    *      - Indicated by messageID equal to 0.
                    *      - call Shutdown.
                    *  4) Another error occured
                    *      - Indicated by an IOException AND notify is not NULL
                    *      - call Shutdown.
                    */
                    if (!_enclosingInstance._clientActive || notify != null)
                    {
                        // #3 & 4
                        _enclosingInstance.Destroy(reason, 0, notify);
                    }
                    else
                    {
                        _enclosingInstance._stopReaderMessageId = ContinueReading;
                    }

                    _enclosingInstance._deadReaderException = readerException;
                    _enclosingInstance._deadReader = _enclosingInstance._reader;
                    _enclosingInstance._reader = null;
                }
            }
        } // End class ReaderThread

        /// <summary>
        ///     Inner class defined so that we can spawn off each unsolicited
        ///     listener as a seperate thread.  We did not want to call the
        ///     unsolicited listener method directly as this would have tied up our
        ///     deamon listener thread in the applications unsolicited listener method.
        ///     Since we do not know what the application unsolicited listener
        ///     might be doing and how long it will take to process the uncoslicited
        ///     notification.  We use this class to spawn off the unsolicited
        ///     notification as a separate thread.
        /// </summary>
        private class UnsolicitedListenerThread : ThreadClass
        {
            private readonly ILdapUnsolicitedNotificationListener _listenerObj;
            private readonly LdapExtendedResponse _unsolicitedMsg;

            internal UnsolicitedListenerThread(ILdapUnsolicitedNotificationListener l, LdapExtendedResponse m)
            {
                _listenerObj = l;
                _unsolicitedMsg = m;
            }

            protected override void Run()
            {
                _listenerObj.MessageReceived(_unsolicitedMsg);
            }
        }

        /// <summary>
        /// Resets the connection to a state where it can be reused.
        /// </summary>
        public void Reset()
        {
            AbandonMessages(null);
        }
    }
}
