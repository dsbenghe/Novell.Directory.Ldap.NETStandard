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
        public static async Task<RootDseInfo> GetRootDseInfoAsync(this ILdapConnection conn)
        {
            var searchResults = await conn
                .SearchAsync(string.Empty, LdapConnection.ScopeBase, "(objectClass=*)", new string[] { "*", "+", "supportedExtension" }, false)
                .ConfigureAwait(false);

            if (await searchResults.HasMoreAsync().ConfigureAwait(false))
            {
                var sr = await searchResults.NextAsync().ConfigureAwait(false);
                return new RootDseInfo(sr);
            }

            return null;
        }
    }
}
