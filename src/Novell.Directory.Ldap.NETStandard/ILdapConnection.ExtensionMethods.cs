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
        public static async Task<RootDseInfo> GetRootDseInfoAsync(this ILdapConnection conn, CancellationToken cancellationToken = default)
        {
            var searchResults = await conn
                .SearchAsync(string.Empty, LdapConnection.ScopeBase, "(objectClass=*)", new string[] { "*", "+", "supportedExtension" }, false, cancellationToken)
                .ConfigureAwait(false);

            var enumerator = searchResults.GetAsyncEnumerator(cancellationToken);
            await using (enumerator.ConfigureAwait(false))
            {
                if (await enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    return new RootDseInfo(enumerator.Current);
                }
            }

            return null;
        }
    }
}
