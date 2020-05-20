using Novell.Directory.Ldap.Sasl;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class TestSaslClientFactory : ISaslClientFactory
    {
        public TestSaslClientFactory(string mechanism)
        {
            SupportedMechanisms = new string[] { mechanism };
        }

        public IReadOnlyList<string> SupportedMechanisms { get; }

        public ISaslClient CreateClient(SaslRequest saslRequest)
        {
            return new TestSaslClient(saslRequest?.SaslMechanism);
        }
    }

    public class TestSaslClient : ISaslClient
    {
        public virtual DebugId DebugId { get; } = DebugId.ForType<TestSaslClient>();
        public TestSaslClient(string mechanism)
        {
            MechanismName = mechanism;
        }

        public string MechanismName { get; }

        public bool HasInitialResponse => false;

        public bool IsComplete => true;

        public void Dispose()
        {
        }

        public byte[] EvaluateChallenge(byte[] challenge)
        {
            return challenge;
        }
    }
}
