using System;
using System.Collections.Generic;
using System.Linq;
using Novell.Directory.Ldap.Controls;
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
                    var entries = lsc.ToList();

                    Assert.Single(entries);
                    ldapEntry.AssertSameAs(entries[0]);
                });
        }

        [Fact]
        [LongRunning]
        public void Search_when_paging_using_VirtualListViewControl_returns_expected_results()
        {
            const int pages = 15;
            const int pageSize = 10;
            var cnPrefix = new Random().Next().ToString();
            var expectedEntries = Enumerable.Range(1, pages * pageSize).Select(x => LdapOps.AddEntry(cnPrefix)).ToList();

            var searchConstraints = new LdapSearchConstraints
            {
                BatchSize = 0,
                MaxResults = 1000
            };

            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    var sortControl = new LdapSortControl(new LdapSortKey("cn"), true);
                    var pageCount = 1;
                    while (true)
                    {
                        searchConstraints.SetControls(new LdapControl[] { BuildLdapVirtualListControl(pageCount, pageSize), sortControl });
                        var searchResults = ldapConnection.Search(TestsConfig.LdapServer.BaseDn, LdapConnection.ScopeSub, "cn=" + cnPrefix + "*", null, false, searchConstraints).ToList();
                        entries.AddRange(searchResults);
                        if (searchResults.Count < pageSize)
                        {
                            break;
                        }

                        pageCount++;
                    }
                });

            Assert.Equal(expectedEntries.Count, entries.Count);
            foreach (var pair in expectedEntries.OrderBy(x => x.Dn).Zip(entries.OrderBy(x => x.Dn)))
            {
                pair.First.AssertSameAs(pair.Second);
            }
        }

        private static LdapVirtualListControl BuildLdapVirtualListControl(int page, int pageSize)
        {
            var startIndex = (page - 1) * pageSize + 1;
            var beforeCount = 0;
            var afterCount = pageSize - 1;
            var contentCount = 0;

            return new LdapVirtualListControl(startIndex, beforeCount, afterCount, contentCount);
        }
    }
}
