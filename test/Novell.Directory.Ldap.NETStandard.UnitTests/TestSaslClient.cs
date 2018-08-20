using Novell.Directory.Ldap.Sasl;
using System.Collections;
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

        public ISaslClient CreateClient(string mechanism, string authorizationId, string serverName, byte[] credentials, Hashtable saslBindProperties)
        {
            return new TestSaslClient(mechanism);
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
