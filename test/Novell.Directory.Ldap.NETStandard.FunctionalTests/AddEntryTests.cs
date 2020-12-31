using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class AddEntryTests
    {
        [Fact]
        public void AddEntry_NotExisting_ShouldWork()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            TestHelper.WithAuthenticatedLdapConnection(ldapConnection => { ldapConnection.Add(ldapEntry); });

            var readEntry = LdapOps.GetEntry(ldapEntry.Dn);
            ldapEntry.AssertSameAs(readEntry);
        }

        [Fact]
        public void AddEntry_AlreadyExists_ShouldThrowEntryAlreadyExists()
        {
            var ldapEntry = LdapOps.AddEntry();

            var ldapException = Assert.Throws<LdapException>(
                () => TestHelper.WithAuthenticatedLdapConnection(ldapConnection => { ldapConnection.Add(ldapEntry); }));
            Assert.Equal(LdapException.EntryAlreadyExists, ldapException.ResultCode);
        }
    }
}
