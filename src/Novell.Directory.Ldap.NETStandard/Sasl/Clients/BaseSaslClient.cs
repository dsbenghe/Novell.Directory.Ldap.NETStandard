using System;
using System.Collections;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    public abstract class BaseSaslClient : ISaslClient
    {
        public abstract DebugId DebugId { get; }
        protected Hashtable Props { get; }
        public QualityOfProtection QualityOfProtection { get; }
        public ProtectionStrength ProtectionStrength { get; }

        protected BaseSaslClient(SaslRequest saslRequest)
        {
            if (saslRequest == null)
            {
                throw new ArgumentNullException(nameof(saslRequest));
            }

            QualityOfProtection = saslRequest.QualityOfProtection;
            ProtectionStrength = saslRequest.ProtectionStrength;
            Props = saslRequest.SaslBindProperties;  // Clone?
        }

        public abstract string MechanismName { get; }
        public abstract bool HasInitialResponse { get; }
        public abstract bool IsComplete { get; }
        public abstract byte[] EvaluateChallenge(byte[] challenge);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public static string GetQOPString(QualityOfProtection qop)
        {
            switch (qop)
            {
                case QualityOfProtection.AuthenticationOnly:
                    return "auth";
                case QualityOfProtection.AuthenticationWithIntegrityProtection:
                    return "auth-int";
                case QualityOfProtection.AuthenticationWithIntegrityAndPrivacyProtection:
                    return "auth-conf";
                default:
                    return string.Empty;
            }
        }
    }
}
