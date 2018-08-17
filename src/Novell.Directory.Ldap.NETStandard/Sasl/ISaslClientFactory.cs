using System.Collections;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.Sasl
{
    public interface ISaslClientFactory
    {
        IReadOnlyList<string> SupportedMechanisms { get; }
        ISaslClient CreateClient(string mechanism, string authorizationId, string serverName, byte[] credentials, Hashtable saslBindProperties);
    }
}
