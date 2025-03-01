using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class PasswordModifyExtendedOperationTests
    {
        [Fact]
        public async Task PasswordModifyExtendedOperation_OfExistingEntry_ShouldWork()
        {
            var existingEntry = await LdapOps.AddEntryAsync();
            var oldPassword = TestsConfig.DefaultPassword;

            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    // Make sure the credentials work
                    await ldapConnection.BindAsync(existingEntry.Dn, oldPassword);

                    var newPassword = "password" + new Random().Next();

                    // Users don't have permission to change their own passwords in the
                    // test environment, so perform the change as the RootUserDn
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.PasswordModifyAsync(existingEntry.Dn, oldPassword, newPassword);

                    // The new password should work
                    await ldapConnection.BindAsync(existingEntry.Dn, newPassword);

                    // The old password should no longer work
                    var ldapException = await Assert.ThrowsAsync<LdapException>(async () => { await ldapConnection.BindAsync(existingEntry.Dn, oldPassword); });

                    Assert.Equal(LdapException.InvalidCredentials, ldapException.ResultCode);
                });
        }
    }
}
