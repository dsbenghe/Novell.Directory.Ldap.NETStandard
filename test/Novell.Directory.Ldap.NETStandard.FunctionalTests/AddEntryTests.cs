using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class AddEntryTests
    {
        [Fact]
        public async Task AddEntry_NotExisting_ShouldWork()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection => { await ldapConnection.AddAsync(ldapEntry); });

            var readEntry = await LdapOps.GetEntryAsync(ldapEntry.Dn);
            ldapEntry.AssertSameAs(readEntry);
        }

        [Fact]
        public async Task AddEntry_AlreadyExists_ShouldThrowEntryAlreadyExists()
        {
            var ldapEntry = await LdapOps.AddEntryAsync();

            var ldapException = await Assert.ThrowsAsync<LdapException>(
                async () => await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection => { await ldapConnection.AddAsync(ldapEntry); }));
            Assert.Equal(LdapException.EntryAlreadyExists, ldapException.ResultCode);
        }
    }
}
