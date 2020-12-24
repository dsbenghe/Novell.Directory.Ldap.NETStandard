using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
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
                var modification = new LdapModification(LdapModification.Replace, newAttribute);
                ldapConnection.Modify(existingEntry.Dn, modification);
            });

            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(existingEntry.Dn, newPassword);
                });
        }
    }
}
