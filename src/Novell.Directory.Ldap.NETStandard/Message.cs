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

//
// Novell.Directory.Ldap.Message.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
    /// <summary> Encapsulates an Ldap message, its state, and its replies.</summary>
    internal class Message
    {
        private readonly string _stackTraceCreation;
        private bool _acceptReplies = true; // false if no longer accepting replies
        private BindProperties _bindprops; // Bind properties if a bind request
        private Connection _conn; // Connection object where msg sent

        private int _mslimit; // client time limit in milliseconds

        // Note: MessageVector is synchronized
        private readonly MessageVector _replies; // place to store replies
        private string _stackTraceCleanup;
        private ThreadClass _timer; // Timeout thread
        private bool _waitForReplyRenamedField = true; // true if wait for reply

        internal Message(LdapMessage msg, int mslimit, Connection conn, MessageAgent agent, BindProperties bindprops)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));

            _stackTraceCreation = Environment.StackTrace;
            _replies = new MessageVector(5);
            Request = msg;
            MessageAgent = agent;
            _mslimit = mslimit;
            MessageId = msg.MessageId;
            _bindprops = bindprops;
        }

        /// <summary>
        ///     Get number of messages queued.
        ///     Don't count the last message containing result code.
        /// </summary>
        internal int Count
        {
            get
            {
                var size = _replies.Count;
                if (Complete)
                {
                    return size > 0 ? size - 1 : size;
                }

                return size;
            }
        }

        /// <summary> sets the agent for this message.</summary>
        internal MessageAgent Agent
        {
            set => MessageAgent = value;
        }

        internal int MessageType
        {
            get
            {
                if (Request == null)
                {
                    return -1;
                }

                return Request.Type;
            }
        }

        internal int MessageId { get; }

        /// <summary>
        ///     gets the operation complete status for this message.
        /// </summary>
        /// <returns>
        ///     the true if the operation is complete, i.e.
        ///     the LdapResult has been received.
        /// </returns>
        internal bool Complete { get; private set; }

        /// <summary>
        ///     Gets the next reply from the reply queue if one exists.
        /// </summary>
        /// <returns>
        ///     the next reply message on the reply queue or null if none.
        /// </returns>
        internal object Reply
        {
            get
            {
                object msg;
                if (_replies == null)
                {
                    return null;
                }

                lock (_replies)
                {
                    // Test and remove must be atomic
                    if (_replies.Count == 0)
                    {
                        return null; // No data
                    }

                    var tempObject = _replies[0];
                    _replies.RemoveAt(0);
                    msg = tempObject; // Atomic get and remove
                }

                if (_conn != null && (Complete || !_acceptReplies) && _replies.Count == 0)
                {
                    // Remove msg from connection queue when last reply read
                    _conn.RemoveMessage(this);
                }

                return msg;
            }
        }

        /// <summary>
        ///     gets the LdapMessage request associated with this message.
        /// </summary>
        /// <returns>
        ///     the LdapMessage request associated with this message.
        /// </returns>
        internal LdapMessage Request { get; private set; }

        internal bool BindRequest => _bindprops != null;

        /// <summary>
        ///     gets the MessageAgent associated with this message.
        /// </summary>
        /// <returns>
        ///     the MessageAgent associated with this message.
        /// </returns>
        internal MessageAgent MessageAgent { get; private set; }

        /// <summary>
        ///     Returns true if replies are queued.
        /// </summary>
        /// <returns>
        ///     false if no replies are queued, otherwise true.
        /// </returns>
        internal bool HasReplies()
        {
            return _replies?.Count > 0;
        }

        /// <summary>
        ///     Gets the next reply from the reply queue or waits until one is there.
        /// </summary>
        /// <returns>
        ///     the next reply message on the reply queue or null.
        /// </returns>
        internal object WaitForReply()
        {
            if (_replies == null)
            {
                return null;
            }

            // sync on message so don't confuse with timer thread
            lock (_replies)
            {
                while (_waitForReplyRenamedField)
                {
                    if (_replies.Count == 0)
                    {
                        Monitor.Wait(_replies);
                        if (_waitForReplyRenamedField)
                        {
                            continue;
                        }

                        break;
                    }

                    var tempObject = _replies[0];
                    _replies.RemoveAt(0);
                    var msg = tempObject;
                    if ((Complete || !_acceptReplies) && _replies.Count == 0)
                    {
                        // Remove msg from connection queue when last reply read
                        _conn.RemoveMessage(this);
                    }

                    return msg;
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns true if replies are accepted for this request.
        /// </summary>
        /// <returns>
        ///     false if replies are no longer accepted for this request.
        /// </returns>
        internal bool AcceptsReplies()
        {
            return _acceptReplies;
        }

        internal void SendMessage()
        {
            _conn.WriteMessage(this);

            // Start the timer thread
            if (_mslimit != 0)
            {
                // Don't start the timer thread for abandon or Unbind
                switch (Request.Type)
                {
                    case LdapMessage.AbandonRequest:
                    case LdapMessage.UnbindRequest:
                        _mslimit = 0;
                        break;

                    default:
                        _timer = new Timeout(_mslimit, this)
                        {
                            IsBackground = true // If this is the last thread running, allow exit.
                        };
                        _timer.Start();
                        break;
                }
            }
        }

        internal void Abandon(LdapConstraints cons, InterThreadException informUserEx)
        {
            if (!_waitForReplyRenamedField)
            {
                return;
            }

            _acceptReplies = false; // don't listen to anyone
            _waitForReplyRenamedField = false; // don't let sleeping threads lie
            if (!Complete)
            {
                try
                {
                    // If a bind, release bind semaphore & wake up waiting threads
                    // Must do before writing abandon message, otherwise deadlock
                    if (_bindprops != null)
                    {
                        int id;
                        if (_conn.BindSemIdClear)
                        {
                            // Semaphore id for normal operations
                            id = MessageId;
                        }
                        else
                        {
                            // Semaphore id for sasl bind
                            id = _conn.BindSemId;
                            _conn.ClearBindSemId();
                        }

                        _conn.FreeWriteSemaphore(id);
                    }

                    // Create the abandon message, but don't track it.
                    LdapControl[] cont = null;
                    if (cons != null)
                    {
                        cont = cons.GetControls();
                    }

                    LdapMessage msg = new LdapAbandonRequest(MessageId, cont);

                    // Send abandon message to server
                    _conn.WriteMessage(msg);
                }
                catch (LdapException ex)
                {
                    Logger.Log.LogWarning("Exception swallowed", ex);
                }

                // If not informing user, remove message from agent
                if (informUserEx == null)
                {
                    MessageAgent.Abandon(MessageId, null);
                }

                _conn.RemoveMessage(this);
            }

            // Get rid of all replies queued
            if (informUserEx != null)
            {
                _replies.Add(new LdapResponse(informUserEx, _conn.ActiveReferral));
                StopTimer();

                // wake up waiting threads to receive exception
                SleepersAwake();

                // Message will get cleaned up when last response removed from queue
            }
            else
            {
                // Wake up any waiting threads, so they can terminate.
                // If informing the user, we wake sleepers after
                // caller queues dummy response with error status
                SleepersAwake();
                Cleanup();
            }
        }

        private void Cleanup()
        {
            StopTimer(); // Make sure timer stopped
            try
            {
                _acceptReplies = false;
                _conn?.RemoveMessage(this);

                // Empty out any accumuluated replies
                _replies?.Clear();
            }
            catch (Exception ex)
            {
                Logger.Log.LogWarning("Exception swallowed", ex);
            }

            _stackTraceCleanup = Environment.StackTrace;

            // Let GC clean up this stuff, leave name in case finalized is called
            _conn = null;
            Request = null;

            // agent = null;  // leave this reference

            // replies = null; //leave this since we use it as a semaphore
            _bindprops = null;
        }

        internal void PutReply(RfcLdapMessage message)
        {
            if (!_acceptReplies)
            {
                return;
            }

            lock (_replies)
            {
                _replies.Add(message);
            }

            message.RequestingMessage = Request; // Save request message info
            switch (message.Type)
            {
                case LdapMessage.SearchResponse:
                case LdapMessage.SearchResultReference:
                case LdapMessage.IntermediateResponse:
                    break;

                default:
                    int res;
                    StopTimer();

                    // Accept no more results for this message
                    // Leave on connection queue so we can abandon if necessary
                    _acceptReplies = false;
                    Complete = true;
                    if (_bindprops != null)
                    {
                        res = ((IRfcResponse)message.Response).GetResultCode().IntValue();
                        if (res != LdapException.SaslBindInProgress)
                        {
                            if (_conn == null)
                            {
                                Logger.Log.LogError(
                                    "Null connection; creation stack {0}, cleanup stack {1}",
                                    _stackTraceCreation, _stackTraceCleanup);
                            }

                            int id;

                            // We either have success or failure on the bind
                            if (res == LdapException.Success)
                            {
                                // Set bind properties into connection object
                                _conn.BindProperties = _bindprops;
                            }

                            // If not a sasl bind in-progress, release the bind
                            // semaphore and wake up all waiting threads
                            if (_conn.BindSemIdClear)
                            {
                                // Semaphore id for normal operations
                                id = MessageId;
                            }
                            else
                            {
                                // Semaphore id for sasl bind
                                id = _conn.BindSemId;
                                _conn.ClearBindSemId();
                            }

                            _conn.FreeWriteSemaphore(id);
                        }
                    }

                    break;
            }

            // wake up waiting threads
            SleepersAwake();
        }

        /// <summary> stops the timeout timer from running.</summary>
        private void StopTimer()
        {
            // If timer thread started, stop it
            _timer?.Stop();
        }

        /// <summary> Notifies all waiting threads.</summary>
        private void SleepersAwake()
        {
            // Notify any thread waiting for this message id
            lock (_replies)
            {
                Monitor.Pulse(_replies);
            }

            // Notify a thread waiting for any message id
            MessageAgent.SleepersAwake(false);
        }

        /// <summary>
        ///     Timer class to provide timing for messages.  Only called
        ///     if time to wait is non zero.
        /// </summary>
        private class Timeout : ThreadClass
        {
            private readonly Message _message;

            private readonly int _timeToWait;

            internal Timeout(int interval, Message msg)
            {
                _timeToWait = interval;
                _message = msg;
            }

            /// <summary>
            ///     The timeout thread.  If it wakes from the sleep, future input
            ///     is stopped and the request is timed out.
            /// </summary>
            protected override void Run()
            {
                for (var i = 0; i < 10000; i++)
                {
                    if (IsStopping)
                    {
                        return;
                    }

                    Thread.Sleep(new TimeSpan(_timeToWait));
                }

                _message._acceptReplies = false;

                // Note: Abandon clears the bind semaphore after failed bind.
                _message.Abandon(
                    null,
                    new InterThreadException("Client request timed out", null, LdapException.LdapTimeout, null,
                        _message));
            }
        }
    }
}
