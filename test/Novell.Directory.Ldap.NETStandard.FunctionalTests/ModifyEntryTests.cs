using System;
using System.Threading.Tasks;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ModifyEntryTests
    {
        [Fact]
        public async Task AddNewAttribute_ToExistingEntry_ShouldWork()
        {
            var existingEntry = await LdapOps.AddEntryAsync();
            var value = Guid.NewGuid().ToString();
            const string attrName = "description";

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                var newAttribute = new LdapAttribute(attrName, value);
                var modification = new LdapModification(LdapModification.Add, newAttribute);
                await ldapConnection.ModifyAsync(existingEntry.Dn, modification);
            });

            var modifiedEntry = await LdapOps.GetEntryAsync(existingEntry.Dn);
            Assert.Equal(value, modifiedEntry.GetAttribute(attrName).StringValue);
        }

        [Fact]
        public async Task ModifyAttributeValue_OfExistingEntry_ShouldWork()
        {
            var existingEntry = await LdapOps.AddEntryAsync();
            var value = Guid.NewGuid().ToString();
            const string attrName = "givenName";

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                var modifiedAttribute = new LdapAttribute(attrName, value);
                var modification = new LdapModification(LdapModification.Replace, modifiedAttribute);
                await ldapConnection.ModifyAsync(existingEntry.Dn, modification);
            });

            var modifiedEntry = await LdapOps.GetEntryAsync(existingEntry.Dn);
            Assert.Equal(value, modifiedEntry.GetAttribute(attrName).StringValue);
        }

        [Fact]
        public async Task Modify_OfNotExistingEntry_ShouldThrowNoSuchObject()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            var ldapException = await Assert.ThrowsAsync<LdapException>(
                () => TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
                {
                    var newAttribute = new LdapAttribute("givenName", "blah");
                    var modification = new LdapModification(LdapModification.Replace, newAttribute);
                    await ldapConnection.ModifyAsync(ldapEntry.Dn, modification);
                }));

            Assert.Equal(LdapException.NoSuchObject, ldapException.ResultCode);
        }
    }
}