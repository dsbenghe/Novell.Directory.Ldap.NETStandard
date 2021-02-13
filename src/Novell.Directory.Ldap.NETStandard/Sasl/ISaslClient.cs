using System;

namespace Novell.Directory.Ldap.Sasl
{
    public interface ISaslClient : IDisposable, IDebugIdentifier
    {
        string MechanismName { get; }

        bool HasInitialResponse { get; }

        byte[] EvaluateChallenge(byte[] challenge);

        bool IsComplete { get; }
    }
}
