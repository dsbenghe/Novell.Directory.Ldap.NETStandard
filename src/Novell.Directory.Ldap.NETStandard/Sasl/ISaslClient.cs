using System;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Sasl
{
    public interface ISaslClient : IDisposable, IDebugIdentifier
    {
        string MechanismName { get; }

        bool HasInitialResponse { get; }

        Task<byte[]> EvaluateChallengeAsync(byte[] challenge);

        bool IsComplete { get; }
    }
}
