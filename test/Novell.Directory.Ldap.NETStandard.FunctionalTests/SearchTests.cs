using System;
using System.Collections.Generic;
using System.Linq;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
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
                    var entries = new List<LdapEntry>();
                    foreach(var entry in lsc)
                    {
                        entries.Add(entry);
                    }

                    Assert.Equal(1, entries.Count);
                    ldapEntry.AssertSameAs(entries[0]);
                });
        }
    }
}
