using Novell.Directory.Ldap.Controls;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Novell.Directory.Ldap.SearchExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class PagedSearchTests : IClassFixture<PagedSearchTests.PagedSearchTestsFixture>
    {
        private readonly PagedSearchTestsFixture _pagedSearchTestsFixture;
        private readonly SearchOptions _searchOptions;
        private readonly LdapSortControl _ldapSortControl;
        private readonly SearchOptions _searchOptionsForZeroResults;

        public PagedSearchTests(PagedSearchTestsFixture pagedSearchTestsFixture)
        {
            _pagedSearchTestsFixture = pagedSearchTestsFixture;
            _searchOptions = new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=" + _pagedSearchTestsFixture.CnPrefix + "*",
                null);
            _ldapSortControl = new LdapSortControl(new LdapSortKey("cn"), true);
            _searchOptionsForZeroResults = new SearchOptions(
                TestsConfig.LdapServer.BaseDn,
                LdapConnection.ScopeSub,
                "cn=blah*",
                null);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public void Search_when_paging_using_VirtualListViewControl_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries = ldapConnection.SearchUsingVlv(
                        _ldapSortControl,
                        _searchOptions,
                        PagedSearchTestsFixture.PageSize
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public void Search_when_paging_using_VirtualListViewControl_using_converter_returns_expected_results()
        {
            var entries = new List<Tuple<LdapEntry>>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries = ldapConnection.SearchUsingVlv(
                        _ldapSortControl,
                        entry => new Tuple<LdapEntry>(entry),
                        _searchOptions,
                        PagedSearchTestsFixture.PageSize
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries.Select(x => x.Item1).ToList());
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public void Search_when_paging_using_VirtualListViewControl_with_one_page_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries = ldapConnection.SearchUsingVlv(
                        _ldapSortControl,
                        _searchOptions,
                        _pagedSearchTestsFixture.Entries.Count
                    );
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact(Skip = "Until configured for openldap")]
        [LongRunning]
        public void Search_when_paging_using_VirtualListViewControl_returns_zero_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries = ldapConnection.SearchUsingVlv(
                        _ldapSortControl,
                        _searchOptionsForZeroResults,
                        PagedSearchTestsFixture.PageSize
                    );
                });

            Assert.Empty(entries);
        }

        [Fact]
        [LongRunning]
        public void Search_when_paging_using_SimplePagedResultControl_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries.AddRange(
                        ldapConnection.SearchUsingSimplePaging(
                            _searchOptions,
                            PagedSearchTestsFixture.PageSize
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact]
        [LongRunning]
        public void Search_when_paging_using_SimplePagedResultControl_using_converter_returns_expected_results()
        {
            var entries = new List<Tuple<LdapEntry>>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries.AddRange(
                        ldapConnection.SearchUsingSimplePaging(
                            entry => new Tuple<LdapEntry>(entry),
                            _searchOptions,
                            PagedSearchTestsFixture.PageSize
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries.Select(x => x.Item1).ToList());
        }

        [Fact]
        [LongRunning]
        public void Search_when_paging_using_SimplePagedResultControl_in_just_one_page_returns_expected_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries.AddRange(
                        ldapConnection.SearchUsingSimplePaging(
                            _searchOptions,
                            _pagedSearchTestsFixture.Entries.Count
                        ));
                });

            AssertReceivedExpectedResults(_pagedSearchTestsFixture.Entries, entries);
        }

        [Fact]
        [LongRunning]
        public void Search_when_paging_using_SimplePagedResultControl_returns_zero_results()
        {
            var entries = new List<LdapEntry>();
            TestHelper.WithAuthenticatedLdapConnection(
                ldapConnection =>
                {
                    entries.AddRange(
                        ldapConnection.SearchUsingSimplePaging(
                            _searchOptionsForZeroResults,
                            PagedSearchTestsFixture.Pages
                        ));
                });

            Assert.Empty(entries);
        }

        private void AssertReceivedExpectedResults(IReadOnlyCollection<LdapEntry> expectedEntries, List<LdapEntry> entries)
        {
            Assert.Equal(expectedEntries.Count, entries.Count);
            foreach (var pair in expectedEntries.OrderBy(x => x.Dn).Zip(entries.OrderBy(x => x.Dn)))
            {
                pair.First.AssertSameAs(pair.Second);
            }
        }

        public sealed class PagedSearchTestsFixture : IDisposable
        {
            public const int Pages = 15;
            public const int PageSize = 20;
            private readonly Random _random = new Random();
            public string CnPrefix { get; }
            public IReadOnlyCollection<LdapEntry> Entries { get; }

            public PagedSearchTestsFixture()
            {
                CnPrefix = _random.Next().ToString();
                Entries = Enumerable.Range(1, (Pages * PageSize) + (_random.Next() % PageSize)).Select(x => LdapOps.AddEntry(CnPrefix)).ToList();
            }

            public void Dispose()
            {
            }
        }
    }
}
