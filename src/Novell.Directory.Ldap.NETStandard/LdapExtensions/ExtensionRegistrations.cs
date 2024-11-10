using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class ExtensionRegistrations
    {
        static ExtensionRegistrations()
        {
            LdapExtendedResponse.Register(LdapKnownOids.Extensions.WhoAmI, message => new LdapWhoAmIResponse(message));
        }

        public static Task<LdapWhoAmIResponse> WhoAmIAsync(this LdapConnection conn, CancellationToken ct = default)
        {
            return conn.WhoAmIAsync(null, ct);
        }

        public static async Task<LdapWhoAmIResponse> WhoAmIAsync(this LdapConnection conn, LdapConstraints cons, CancellationToken ct = default)
        {
            var result = await conn.ExtendedOperationAsync(new LdapWhoAmIOperation(), cons, ct).ConfigureAwait(false);
            if (result is LdapWhoAmIResponse whoami)
            {
                return whoami;
            }

            return new LdapWhoAmIResponse(result.Message);
        }
    }
}
