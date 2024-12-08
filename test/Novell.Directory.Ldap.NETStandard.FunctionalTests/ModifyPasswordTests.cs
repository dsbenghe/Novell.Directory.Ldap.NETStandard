using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ModifyPasswordTests
    {
        [Fact]
        public async Task ModifyPassword_OfExistingEntry_ShouldWork()
        {
            var existingEntry = await LdapOps.AddEntryAsync();
            var newPassword = "password" + new Random().Next();

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                var newAttribute = new LdapAttribute("userPassword", newPassword);
                var modification = new LdapModification(LdapModification.Replace, newAttribute);
                await ldapConnection.ModifyAsync(existingEntry.Dn, modification);
            });

            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(existingEntry.Dn, newPassword);

                    // Check to see if the password modify extension is supported and test that too
                    var rootDse = await ldapConnection.GetRootDseInfoAsync();
                    if (rootDse.SupportsExtension(LdapKnownOids.Extensions.PasswordModify))
                    {
                        var oldPassword = newPassword;
                        newPassword = "password" + new Random().Next();

                        // Users don't have permission to change their own passwords in the
                        // test environment so perform the change as the RootUserDn
                        await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                        await ldapConnection.PasswordModifyAsync(existingEntry.Dn, oldPassword, newPassword);

                        await ldapConnection.BindAsync(existingEntry.Dn, newPassword);
                    }
                });
        }
    }
}
