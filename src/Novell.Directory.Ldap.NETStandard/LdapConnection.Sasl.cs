using Novell.Directory.Ldap.Rfc2251;
using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Utilclass;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap
{
    public partial class LdapConnection : ILdapConnection
    {
        private readonly ConcurrentDictionary<string, ISaslClientFactory> _saslClientFactories;

        public IReadOnlyCollection<ISaslClientFactory> GetRegisteredSaslClientFactories()
            => _saslClientFactories.Values.ToList();

        public void RegisterSaslClientFactory(ISaslClientFactory saslClientFactory)
        {
            if (saslClientFactory == null)
            {
                throw new ArgumentNullException(nameof(saslClientFactory));
            }

            var mechanisms = saslClientFactory.SupportedMechanisms;
            if (mechanisms.IsEmpty())
            {
                throw new ArgumentException("A SASL Client Factory must support at least one mechanism.", nameof(saslClientFactory));
            }

            foreach (var mechanism in mechanisms)
            {
                var factoryInDict = _saslClientFactories.GetOrAdd(mechanism, saslClientFactory);
                if (factoryInDict != saslClientFactory)
                {
                    var typeName = factoryInDict.GetType().Name;
                    var msg = $"There is already a SASL Client Factory registered for '{mechanism}': {typeName}";
                    throw new InvalidOperationException(msg);
                }
            }
        }

        public bool IsSaslMechanismSupported(string mechanism)
        {
            // Registered Mechanisms always take precedence over the default
            if (_saslClientFactories?.ContainsKey(mechanism) == true)
            {
                return true;
            }

            return DefaultSaslClientFactory.IsSaslMechanismSupported(mechanism);
        }

        /// <summary>
        /// Internal for Unit-Test purposes only.
        /// </summary>
        internal ISaslClient CreateClient(SaslRequest saslRequest)
        {
            if (saslRequest == null)
            {
                throw new ArgumentNullException(nameof(saslRequest));
            }

            if (_saslClientFactories.TryGetValue(saslRequest.SaslMechanism, out var factory))
            {
                return factory.CreateClient(saslRequest);
            }

            return DefaultSaslClientFactory.CreateClient(saslRequest);
        }

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
        ///     TODO: Can this be a strong class rather than a Hashtable/IDictionary?.
        /// </remarks>
        public virtual IDictionary SaslBindProperties
            => Connection?.BindProperties?.SaslBindProperties;

        public virtual void Bind(SaslRequest saslRequest)
        {
            if (saslRequest == null)
            {
                throw new ArgumentNullException(nameof(saslRequest));
            }

            Hashtable saslBindProperties = null;

            using (var saslClient = CreateClient(saslRequest))
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
                            var replyBuf = SendLdapSaslBindRequest(clientResponse, saslClient.MechanismName, bindProps, constraints);

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

        private byte[] SendLdapSaslBindRequest(byte[] toWrite, string mechanism, BindProperties bindProps, LdapConstraints constraints)
        {
            constraints = constraints ?? _defSearchCons;
            var msg = new LdapSaslBindRequest(LdapV3, mechanism, constraints.GetControls(), toWrite);

            var queue = SendRequestToServer(msg, constraints.TimeLimit, null, bindProps);
            if (!(queue.GetResponse() is LdapResponse ldapResponse))
            {
                throw new LdapException("Bind failure, no response received.");
            }

            var bindResponse = (RfcBindResponse)ldapResponse.Asn1Object.get_Renamed(1);
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
