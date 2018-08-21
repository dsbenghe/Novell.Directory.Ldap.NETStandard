using System.Collections.Generic;

namespace Novell.Directory.Ldap.Sasl
{
    public interface ISaslClientFactory
    {
        IReadOnlyList<string> SupportedMechanisms { get; }
        ISaslClient CreateClient(SaslRequest saslRequest);
    }
}
