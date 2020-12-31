using Novell.Directory.Ldap.Sasl.Clients;
using System;

namespace Novell.Directory.Ldap.Sasl
{
    public static class DefaultSaslClientFactory // static, thus not implementing ISaslClientFactory. Should be be non-static and do?
    {
        public static ISaslClient CreateClient(SaslRequest saslRequest)
        {
            if (saslRequest == null)
            {
                throw new ArgumentNullException(nameof(saslRequest));
            }

            if (!IsSaslMechanismSupported(saslRequest.SaslMechanism))
            {
                return null;
            }

            switch (saslRequest.SaslMechanism)
            {
                case SaslConstants.Mechanism.CramMd5:
                    return new CramMD5Client(saslRequest);
                case SaslConstants.Mechanism.DigestMd5:
                    return new DigestMD5Client(saslRequest);
                case SaslConstants.Mechanism.Plain:
                    return new PlainClient(saslRequest);
                case SaslConstants.Mechanism.External:
                    return new ExternalClient(saslRequest);

                // case LdapConstants.SaslMechanism.GssApi:
                default:
                    return null;
            }
        }

        public static bool IsSaslMechanismSupported(string mechanism)
        {
            if (string.IsNullOrEmpty(mechanism))
            {
                return false;
            }

            switch (mechanism)
            {
                case SaslConstants.Mechanism.CramMd5:
                case SaslConstants.Mechanism.DigestMd5:
                case SaslConstants.Mechanism.Plain:
                case SaslConstants.Mechanism.External:
                    return true;
                default:
                    return false;
            }
        }
    }
}
