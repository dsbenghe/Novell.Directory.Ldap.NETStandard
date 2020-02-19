using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class SearchTests
    {
        [Fact]
        public async Task Can_Search_ByCn()
        {
            const int noOfEntries = 10;
            var ldapEntries = Enumerable.Range(1, noOfEntries).Select(async x => await LdapOps.AddEntryAsync()).ToList();
            var ldapEntry = await ldapEntries[new Random().Next() % noOfEntries];
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    var lsc = await ldapConnection.SearchAsync(TestsConfig.LdapServer.BaseDn, LdapConnection.ScopeSub, "cn=" + ldapEntry.GetAttribute("cn").StringValue, null, false);
                    var entries = lsc.ToList();

                    Assert.Single(entries);
                    ldapEntry.AssertSameAs(entries[0]);
                });
        }
    }
}
