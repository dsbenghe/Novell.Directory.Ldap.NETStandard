namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class LdapOps
    {
        public static LdapEntry AddEntry(string cnPrefix = null)
        {
            return TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                var ldapEntry = LdapEntryHelper.NewLdapEntry(cnPrefix);
                ldapConnection.Add(ldapEntry);
                return ldapEntry;
            });
        }

        public static LdapEntry GetEntry(string dn)
        {
            try
            {
                return TestHelper.WithAuthenticatedLdapConnection(ldapConnection => ldapConnection.Read(dn));
            }
            catch (LdapException ldapException) when (ldapException.ResultCode == LdapException.NoSuchObject /* not found */)
            {
                return null;
            }
        }
    }
}
