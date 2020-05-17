using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class RenameEntryTests
    {
        [Fact]
        public async Task Rename_ExistingEntry_ShouldWork()
        {
            var entry = await LdapOps.AddEntryAsync();
            var newCn = Guid.NewGuid().ToString();

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                await ldapConnection.RenameAsync(entry.Dn, "cn=" + newCn, true);
            });

            Assert.Null(await LdapOps.GetEntryAsync(entry.Dn));
            var renamedEntry = await LdapOps.GetEntryAsync(TestHelper.BuildDn(newCn));
            Assert.NotNull(renamedEntry);
            entry.GetAttributeSet().AssertSameAs(renamedEntry.GetAttributeSet(), new List<string> {"cn" });
        }
    }
}
