using System;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ModifyEntryTests
    {
        [Fact]
        public void AddNewAttribute_ToExistingEntry_ShouldWork()
        {
            var existingEntry = LdapOps.AddEntry();
            var value = Guid.NewGuid().ToString();
            const string attrName = "description";

            TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                var newAttribute = new LdapAttribute(attrName, value);
                var modification = new LdapModification(LdapModification.ADD, newAttribute);
                ldapConnection.Modify(existingEntry.DN, modification);
            });

            var modifiedEntry = LdapOps.GetEntry(existingEntry.DN);
            Assert.Equal(value, modifiedEntry.getAttribute(attrName).StringValue);
        }

        [Fact]
        public void ModifyAttributeValue_OfExistingEntry_ShouldWork()
        {
            var existingEntry = LdapOps.AddEntry();
            var value = Guid.NewGuid().ToString();
            const string attrName = "givenName";

            TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                var modifiedAttribute = new LdapAttribute(attrName, value);
                var modification = new LdapModification(LdapModification.REPLACE, modifiedAttribute);
                ldapConnection.Modify(existingEntry.DN, modification);
            });

            var modifiedEntry = LdapOps.GetEntry(existingEntry.DN);
            Assert.Equal(value, modifiedEntry.getAttribute(attrName).StringValue);
        }

        [Fact]
        public void Modify_OfNotExistingEntry_ShouldThrowNoSuchObject()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            var ldapException = Assert.Throws<LdapException>(
                () => TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
                {
                    var newAttribute = new LdapAttribute("givenName", "blah");
                    var modification = new LdapModification(LdapModification.REPLACE, newAttribute);
                    ldapConnection.Modify(ldapEntry.DN, modification);
                })
            );

            Assert.Equal(LdapException.NO_SUCH_OBJECT, ldapException.ResultCode);
        }
    }
}