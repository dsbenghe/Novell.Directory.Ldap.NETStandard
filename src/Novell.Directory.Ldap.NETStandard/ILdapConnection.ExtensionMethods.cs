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
        public static RootDseInfo GetRootDseInfo(this ILdapConnection conn)
        {
            var searchResults = conn.Search(string.Empty, LdapConnection.ScopeBase, "(objectClass=*)", new[] { "*", "+", "supportedExtension" }, false);
            if (searchResults.HasMore())
            {
                var sr = searchResults.Next();
                return new RootDseInfo(sr);
            }

            return null;
        }
    }
}
