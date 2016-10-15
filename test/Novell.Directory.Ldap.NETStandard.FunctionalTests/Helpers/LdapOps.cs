namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class LdapOps
    {
        public static LdapEntry AddEntry()
        {
            return TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                var ldapEntry = LdapEntryHelper.NewLdapEntry();
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
            catch (LdapException ldapException) when (ldapException.ResultCode == LdapException.NO_SUCH_OBJECT /* not found */)
            {
                return null;
            }
        }
    }
}