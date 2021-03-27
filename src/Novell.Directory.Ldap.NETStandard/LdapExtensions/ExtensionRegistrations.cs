using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class ExtensionRegistrations
    {
        static ExtensionRegistrations()
        {
            LdapExtendedResponse.Register(LdapKnownOids.Extensions.WhoAmI, typeof(LdapWhoAmIResponse));
        }

        public static async Task<LdapWhoAmIResponse> WhoAmIAsync(this LdapConnection conn, LdapConstraints cons = null, CancellationToken cancellationToken = default)
        {
            var result = await conn.ExtendedOperationAsync(new LdapWhoAmIOperation(), cons, cancellationToken).ConfigureAwait(false);
            if (result is LdapWhoAmIResponse whoami)
            {
                return whoami;
            }

            return new LdapWhoAmIResponse(result.Message);
        }
    }
}
