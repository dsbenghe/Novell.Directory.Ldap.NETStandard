using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections;
using System.Text;

namespace Novell.Directory.Ldap
{
    public partial class LdapConnection : ILdapConnection
    {
        /// <summary>
        ///     Returns the properties if any specified on binding with a
        ///     SASL mechanism.
        ///     Null is returned if no authentication has been performed
        ///     or no authentication Map is present.
        /// </summary>
        /// <returns>
        ///     The bind properties Map Object used for SASL bind or null if
        ///     the connection is not present or not authenticated.
        /// </returns>
        /// <remarks>
        ///     TODO: Can this be a strong class rather than a Hashtable/IDictionary?
        /// </remarks>
        public virtual IDictionary SaslBindProperties
            => Connection?.BindProperties?.SaslBindProperties;

        public virtual void Bind(SaslRequest saslRequest)
        {
            if (saslRequest == null) throw new ArgumentNullException(nameof(saslRequest));

            Hashtable saslBindProperties = null;

            using (var saslClient = SaslClientFactory.CreateLdapClient(saslRequest.SaslMechanism, saslRequest.AuthorizationId, Host, saslRequest.Credentials, saslBindProperties))
            {
                if (saslClient == null)
                {
                    throw new ArgumentException("Unsupported Sasl Authentication mechanism: " + saslRequest.SaslMechanism);
                }

                var constraints = saslRequest.Constraints ?? _defSearchCons;

                try
                {
                    var bindProps = new BindProperties(LdapV3, saslRequest.AuthorizationId, "sasl", anonymous: false, bindProperties: saslBindProperties);
                    var bindSemId = Connection.AcquireWriteSemaphore();
                    Connection.SetBindSemId(bindSemId);

                    byte[] clientResponse = null;
                    if (saslClient.HasInitialResponse)
                    {
                        clientResponse = saslClient.EvaluateChallenge(Array.Empty<byte>());
                    }

                    while (!saslClient.IsComplete)
                    {
                        try
                        {
                            var replyBuf = SendLdapSaslBindRequest(clientResponse, saslClient.MechanismName, bindProps);

                            if (replyBuf != null)
                            {
                                clientResponse = saslClient.EvaluateChallenge(replyBuf);
                            }
                            else
                            {
                                clientResponse = saslClient.EvaluateChallenge(Array.Empty<byte>());
                            }

                        }
                        catch (Exception ex)
                        {
                            throw new LdapException("Unexpected SASL error.", LdapException.Other, null, ex);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new LdapException(e);
                }
            }
        }

        private byte[] SendLdapSaslBindRequest(byte[] toWrite, string mechanism, BindProperties bindProps)
        {
            var cons = _defSearchCons;       
            var msg = new LdapSaslBindRequest(LdapV3, mechanism, cons.GetControls(), toWrite);

            var queue = SendRequestToServer(msg, cons.TimeLimit, null, bindProps);
            if (!(queue.GetResponse() is LdapResponse ldapResponse))
            {
                throw new LdapException("Bind failure, no response received.");
            }

            var bindResponse = ((Rfc2251.RfcBindResponse)ldapResponse.Asn1Object.get_Renamed(1));
            lock (_responseCtlSemaphore)
            {
                _responseCtls = ldapResponse.Controls;
            }
            var serverCreds = bindResponse.ServerSaslCreds;
            var resultCode = ldapResponse.ResultCode;

            byte[] replyBuf = null;
            if (resultCode == LdapException.SaslBindInProgress || resultCode == LdapException.Success)
            {
                if (serverCreds != null)
                {
                    replyBuf = serverCreds.ByteValue();
                }
            }
            else
            {
                ldapResponse.ChkResultCode();
                throw new LdapException("SASL Bind Error.", resultCode, null);
            }
            return replyBuf;
        }
    }
}
