using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
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
                var modification = new LdapModification(LdapModification.Add, newAttribute);
                ldapConnection.Modify(existingEntry.Dn, modification);
            });

            var modifiedEntry = LdapOps.GetEntry(existingEntry.Dn);
            Assert.Equal(value, modifiedEntry.GetAttribute(attrName).StringValue);
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
                var modification = new LdapModification(LdapModification.Replace, modifiedAttribute);
                ldapConnection.Modify(existingEntry.Dn, modification);
            });

            var modifiedEntry = LdapOps.GetEntry(existingEntry.Dn);
            Assert.Equal(value, modifiedEntry.GetAttribute(attrName).StringValue);
        }

        [Fact]
        public void Modify_OfNotExistingEntry_ShouldThrowNoSuchObject()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            var ldapException = Assert.Throws<LdapException>(
                () => TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
                {
                    var newAttribute = new LdapAttribute("givenName", "blah");
                    var modification = new LdapModification(LdapModification.Replace, newAttribute);
                    ldapConnection.Modify(ldapEntry.Dn, modification);
                }));

            Assert.Equal(LdapException.NoSuchObject, ldapException.ResultCode);
        }
    }
}
