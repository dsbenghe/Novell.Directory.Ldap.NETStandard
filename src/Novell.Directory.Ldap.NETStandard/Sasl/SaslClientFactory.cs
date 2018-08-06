using System.Collections;
using System.Collections.Generic;
using Novell.Directory.Ldap.Sasl.Clients;

namespace Novell.Directory.Ldap.Sasl
{
    public static class SaslClientFactory
    {
        public static ISaslClient CreateClient(string mechanism, string authorizationId, string protocol, string serverName, byte[] credentials, Hashtable props)
        {
            if (string.IsNullOrEmpty(mechanism))
            {
                return null;
            }

            switch (mechanism.ToUpperInvariant())
            {
                case SaslConstants.Mechanism.CramMd5:
                    return CramMD5Client.CreateClient(authorizationId, protocol, serverName, credentials, props);

                //case LdapConstants.SaslMechanism.DigestMd5:
                //case LdapConstants.SaslMechanism.Plain:
                //case LdapConstants.SaslMechanism.GssApi:
                default:
                    return null;
            }
        }

        public static ISaslClient CreateClient(IReadOnlyCollection<string> mechanisms, string authorizationId, string protocol, string serverName, byte[] credentials, Hashtable props)
        {
            if (mechanisms.IsEmpty())
            {
                return null;
            }

            foreach (var mechanism in mechanisms)
            {
                var client = CreateClient(mechanism, authorizationId, protocol, serverName, credentials, props);
                if (client != null)
                {
                    return client;
                }
            }

            return null;
        }
    }
}
