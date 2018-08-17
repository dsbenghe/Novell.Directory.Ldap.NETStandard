using System.Collections;
using Novell.Directory.Ldap.Sasl.Clients;

namespace Novell.Directory.Ldap.Sasl
{
    public static class DefaultSaslClientFactory // static, thus not implementing ISaslClientFactory. Should be be non-static and do?
    {
        public static ISaslClient CreateClient(string mechanism, string authorizationId, string serverName, byte[] credentials, Hashtable props)
        {
            if (!IsSaslMechanismSupported(mechanism))
            {
                return null;
            }

            switch (mechanism.ToUpperInvariant()) // TODO: Remove this ToUpperInvariant
            {
                case SaslConstants.Mechanism.CramMd5:
                    return CramMD5Client.CreateClient(authorizationId, serverName, credentials, props);

                //case LdapConstants.SaslMechanism.DigestMd5:
                //case LdapConstants.SaslMechanism.Plain:
                //case LdapConstants.SaslMechanism.GssApi:
                default:
                    return null;
            }
        }

        public static bool IsSaslMechanismSupported(string mechanism)
        {
            if (string.IsNullOrEmpty(mechanism)) return false;

            switch (mechanism.ToUpperInvariant()) // TODO: Remove this ToUpperInvariant
            {
                case SaslConstants.Mechanism.CramMd5:
                    return true;
                default:
                    return false;
            }
        }
    }
}
