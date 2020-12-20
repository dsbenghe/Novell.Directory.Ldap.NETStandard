using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class SearchTests
    {
        [Fact]
        public void Can_Search_ByCn()
        {
            const int noOfEntries = 10;
            var ldapEntries = Enumerable.Range(1, noOfEntries).Select(x => LdapOps.AddEntry()).ToList();
            var ldapEntry = ldapEntries[new Random().Next() % noOfEntries];
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    var lsc = ldapConnection.Search(TestsConfig.LdapServer.BaseDn, LdapConnection.ScopeSub, "cn=" + ldapEntry.GetAttribute("cn").StringValue, null, false);
                    var entries = lsc.ToList();

                    Assert.Single(entries);
                    ldapEntry.AssertSameAs(entries[0]);
                });
        }
    }
}
