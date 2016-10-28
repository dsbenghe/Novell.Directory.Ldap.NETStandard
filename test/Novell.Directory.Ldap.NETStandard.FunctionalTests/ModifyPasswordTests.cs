using System;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ModifyPasswordTests
    {
        [Fact]
        public void ModifyPassword_OfExistingEntry_ShouldWork()
        {
            var existingEntry = LdapOps.AddEntry();
            var newPassword = "password" + new Random().Next();

            TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {                
                var newAttribute = new LdapAttribute("userPassword", newPassword);
                var modification = new LdapModification(LdapModification.REPLACE, newAttribute);
                ldapConnection.Modify(existingEntry.DN, modification);
            });

            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(existingEntry.DN, newPassword);
                });
        }
    }
}
