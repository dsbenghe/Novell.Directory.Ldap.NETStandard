using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class RenameEntryTests
    {
        [Fact]
        public void Rename_ExistingEntry_ShouldWork()
        {
            var entry = LdapOps.AddEntry();
            var newCn = Guid.NewGuid().ToString();

            TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                ldapConnection.Rename(entry.Dn, "cn=" + newCn, true);
            });

            Assert.Null(LdapOps.GetEntry(entry.Dn));
            var renamedEntry = LdapOps.GetEntry(TestHelper.BuildDn(newCn));
            Assert.NotNull(renamedEntry);
            entry.GetAttributeSet().AssertSameAs(renamedEntry.GetAttributeSet(), new List<string> { "cn" });
        }
    }
}
