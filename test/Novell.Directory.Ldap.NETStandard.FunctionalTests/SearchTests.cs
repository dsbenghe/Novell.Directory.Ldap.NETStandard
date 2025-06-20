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
            const int NoOfEntries = 10;
            var ldapEntries = Enumerable.Range(1, NoOfEntries).Select(x => LdapOps.AddEntryAsync().GetAwaiter().GetResult()).ToList();
            var ldapEntry = ldapEntries[new Random().Next() % NoOfEntries];
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    var entries = await ldapConnection.SearchAsyncAsList(
                        TestsConfig.LdapServer.BaseDn,
                        LdapConnection.ScopeSub,
                        "cn=" + ldapEntry.Get("cn").StringValue,
                        null,
                        false);

                    Assert.Single(entries);
                    ldapEntry.AssertSameAs(entries[0]);
                });
        }
    }
}
