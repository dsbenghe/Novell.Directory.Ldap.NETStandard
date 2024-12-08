using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    public static class ExtensionRegistrations
    {
        static ExtensionRegistrations()
        {
            LdapExtendedResponse.Register(LdapKnownOids.Extensions.WhoAmI, message => new LdapWhoAmIResponse(message));
            LdapExtendedResponse.Register(LdapKnownOids.Extensions.PasswordModify, message => new LdapPasswordModifyResponse(message));
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

        public static Task<LdapPasswordModifyResponse> PasswordModifyAsync(
            this ILdapConnection conn, string userIdentity, string oldPasswd, string newPasswd, CancellationToken ct = default)
        {
            return conn.PasswordModifyAsync(null, userIdentity, oldPasswd, newPasswd, ct);
        }

        public static async Task<LdapPasswordModifyResponse> PasswordModifyAsync(
            this ILdapConnection conn, LdapConstraints cons, string userIdentity, string oldPasswd, string newPasswd, CancellationToken ct = default)
        {
            var result = await conn.ExtendedOperationAsync(new LdapPasswordModifyOperation(userIdentity, oldPasswd, newPasswd), cons, ct).ConfigureAwait(false);
            if (result is LdapPasswordModifyResponse pwmod)
            {
                return pwmod;
            }

            return new LdapPasswordModifyResponse(result.Message);
        }
    }
}
