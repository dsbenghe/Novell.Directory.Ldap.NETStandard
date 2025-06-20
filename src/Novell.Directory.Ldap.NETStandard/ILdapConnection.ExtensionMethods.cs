using Novell.Directory.Ldap.Utilclass;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// Extension Methods for <see cref="ILdapConnection"/> to
    /// avoid bloating that interface.
    /// </summary>
    public static class LdapConnectionExtensionMethods
    {
        /// <summary>
        /// Get some common Attributes from the Root DSE.
        /// This is really just a specialized <see cref="LdapSearchRequest"/>
        /// to handle getting some commonly requested information.
        /// </summary>
        public static async Task<RootDseInfo> GetRootDseInfoAsync(this ILdapConnection conn, CancellationToken ct = default)
        {
            var searchResults = await conn
                .SearchAsync(string.Empty, LdapConnection.ScopeBase, "(objectClass=*)", new string[] { "*", "+", "supportedExtension" }, false, ct)
                .ConfigureAwait(false);

            await foreach (var searchResult in searchResults.ConfigureAwait(false))
            {
                return new RootDseInfo(searchResult);
            }

            return null;
        }

        /// <summary>
        /// Convenience method to avoid double await.
        /// </summary>
        public static async Task<List<LdapEntry>> SearchAsyncAsList(this ILdapConnection conn, string @base, int scope, string filter, string[] attrs, bool typesOnly,
            CancellationToken ct = default)
        {
            var lsr = await conn.SearchAsync(@base, scope, filter, attrs, typesOnly, ct);
            return await lsr.ToListAsync(cancellationToken: ct);
        }

        /// <summary>
        /// Convenience method to avoid double await.
        /// </summary>
        public static async Task<List<LdapEntry>> SearchAsyncAsList(this ILdapConnection conn, string @base, int scope, string filter, string[] attrs, bool typesOnly,
            LdapSearchConstraints cons,  CancellationToken ct = default)
        {
            var lsr = await conn.SearchAsync(@base, scope, filter, attrs, typesOnly, cons, ct);
            return await lsr.ToListAsync(cancellationToken: ct);
        }
    }
}
