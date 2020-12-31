using System.Threading.Tasks;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class LdapOps
    {
        public static async Task<LdapEntry> AddEntryAsync(string cnPrefix = null)
        {
            return await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                var ldapEntry = LdapEntryHelper.NewLdapEntry(cnPrefix);
                await ldapConnection.AddAsync(ldapEntry);
                return ldapEntry;
            });
        }

        public static async Task<LdapEntry> GetEntryAsync(string dn)
        {
            try
            {
                return await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
                    await ldapConnection.ReadAsync(dn));
            }
            catch (LdapException ldapException) when (ldapException.ResultCode == LdapException.NoSuchObject /* not found */)
            {
                return null;
            }
        }
    }
}
