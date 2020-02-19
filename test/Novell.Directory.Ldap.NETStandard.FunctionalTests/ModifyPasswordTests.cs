using System;
using System.Threading.Tasks;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
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
                });
        }
    }
}
