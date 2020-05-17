using System;
using System.Threading.Tasks;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class DeleteEntryTests
    {
        [Fact]
        public async Task Delete_OfExistingEntry_ShouldWork()
        {
            var existingEntry = await LdapOps.AddEntryAsync();

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection => { await ldapConnection.DeleteAsync(existingEntry.Dn); });

            var retrievedEntry = await LdapOps.GetEntryAsync(existingEntry.Dn);
            Assert.Null(retrievedEntry);
        }

        [Fact]
        public async Task  Delete_OfNotExistingEntry_ShouldThrownNoSuchObject()
        {
            var ldapException = await Assert.ThrowsAsync<LdapException>(
                () => TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection => { await ldapConnection.DeleteAsync(TestHelper.BuildDn(Guid.NewGuid().ToString())); }));
            Assert.Equal(LdapException.NoSuchObject, ldapException.ResultCode);
        }
    }
}