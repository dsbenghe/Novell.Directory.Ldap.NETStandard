using System;
using System.Collections.Generic;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
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

            TestHelper.WithAuthenticatedLdapConnection((ldapConnection) =>
            {
                ldapConnection.Rename(entry.DN, "cn=" + newCn, true);
            });

            Assert.Null(LdapOps.GetEntry(entry.DN));
            var renamedEntry = LdapOps.GetEntry(TestHelper.BuildDn(newCn));
            Assert.NotNull(renamedEntry);
            entry.getAttributeSet().AssertSameAs(renamedEntry.getAttributeSet(), new List<string> {"cn"});
        }
    }
}
