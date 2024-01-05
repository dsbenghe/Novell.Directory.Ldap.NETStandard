using Novell.Directory.Ldap.Controls;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Novell.Directory.Ldap.SearchExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class PagedSearchLargeResultSetsTests : IClassFixture<PagedSearchLargeResultSetsTests.PagedSearchTestsAsyncFixture>
    {
        private readonly PagedSearchTestsAsyncFixture _pagedSearchTestsFixture;
        private readonly SearchOptions _searchOptions;
        private readonly LdapSortControl _ldapSortControl = new LdapSortControl(new LdapSortKey("cn"), true);
        private readonly SearchOptions _searchOptionsForZeroResults = new SearchOptions(
            TestsConfig.LdapServer.BaseDn,
            LdapConnection.ScopeSub,
            "cn=blah*",
            null);

        public PagedSearchLargeResultSetsTests(PagedSearchTestsAsyncFixture pagedSearchTestsFixture)
        {
            _pagedSearchTestsFixture = pagedSearchTestsFixture;
            _searchOptions = CreateSearchOptions();
        }

        [Fact]
        [LongRunning]
        public async Task Search_when_paging_using_SimplePagedResultControl_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            await TestHelper.WithAuthenticatedLdapConnectionAsync(
                async ldapConnection =>
                {
                    entries.AddRange(
                        await ldapConnection.SearchUsingSimplePagingAsync(
                            _searchOptions,
                            _pagedSearchTestsFixture.PageSize
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        private void AssertReceivedExpectedResults(IReadOnlyCollection<LdapEntry> expectedEntries, List<LdapEntry> entries)
        {
            Assert.Equal(expectedEntries.Count, entries.Count);
            foreach (var pair in expectedEntries.OrderBy(x => x.Dn).Zip(entries.OrderBy(x => x.Dn)))
            {
                pair.First.AssertSameAs(pair.Second);
            }
        }

        private SearchOptions CreateSearchOptions()
        {
            return new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=" + _pagedSearchTestsFixture.CnPrefix + "*",
                null);
        }

        public class PagedSearchTestsAsyncFixture : PagedSearchTestsAsyncFixtureBase
        {
            public PagedSearchTestsAsyncFixture()
                : base(120, 100)
            {
            }
        }
    }
}
