using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// Helper to do paged searches
    /// with <see cref="SimplePagedResultsControl"/>.
    /// </summary>
    public class SimplePagedResultsControlHandler
    {
        [NotNull]
        private readonly ILdapConnection _ldapConnection;

        public SimplePagedResultsControlHandler([NotNull] ILdapConnection ldapConnection)
        {
            _ldapConnection = ldapConnection ?? throw new ArgumentNullException(nameof(ldapConnection));
        }

        public List<LdapEntry> SearchWithSimplePaging([NotNull] SearchOptions options, int pageSize)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return SearchWithSimplePaging(entry => entry, options, pageSize);
        }

        public List<T> SearchWithSimplePaging<T>([NotNull] Func<LdapEntry, T> converter, [NotNull] SearchOptions options, int pageSize)
        {
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var searchResult = new List<T>();
            var searchConstraints = options.SearchConstraints ?? _ldapConnection.SearchConstraints;
            var isNextPageAvailable = PrepareForNextPage(null, pageSize, true, ref searchConstraints);
            while (isNextPageAvailable)
            {
                var responseControls = RetrievePage(options, searchConstraints, searchResult, converter);
                isNextPageAvailable = PrepareForNextPage(responseControls, pageSize, false, ref searchConstraints);
            }

            return searchResult;
        }

        private static bool PrepareForNextPage(
            [CanBeNull] LdapControl[] pageResponseControls,
            int pageSize,
            bool isInitialCall,
            ref LdapSearchConstraints searchConstraints)
        {
            var cookie = SimplePagedResultsControl.GetEmptyCookie;
            if (!isInitialCall)
            {
                var pagedResultsControl = (SimplePagedResultsControl)pageResponseControls?.SingleOrDefault(x => x is SimplePagedResultsControl);
                if (pagedResultsControl == null)
                {
                    throw new LdapException($"Failed to find <{nameof(SimplePagedResultsControl)}>. Searching is abruptly stopped");
                }

                // server signaled end of result set
                if (pagedResultsControl.IsEmptyCookie())
                {
                    return false;
                }

                cookie = pagedResultsControl.Cookie;
            }

            searchConstraints = ApplyPagedResultsControl(searchConstraints, pageSize, cookie);
            return true;
        }

        private static LdapSearchConstraints ApplyPagedResultsControl(LdapSearchConstraints searchConstraints, int pageSize, [CanBeNull] byte[] cookie)
        {
            var ldapPagedControl = new SimplePagedResultsControl(pageSize, cookie);
            searchConstraints.BatchSize = 0;
            searchConstraints.SetControls(ldapPagedControl);
            return searchConstraints;
        }

        private LdapControl[] RetrievePage<T>(
            [NotNull] SearchOptions options,
            [NotNull] LdapSearchConstraints searchConstraints,
            [NotNull] List<T> mappedResultsAccumulator,
            [NotNull] Func<LdapEntry, T> converter)
        {
            if (searchConstraints == null)
            {
                throw new ArgumentNullException(nameof(searchConstraints));
            }

            if (mappedResultsAccumulator == null)
            {
                throw new ArgumentNullException(nameof(mappedResultsAccumulator));
            }

            var searchResults = _ldapConnection.Search(
                    options.SearchBase,
                    LdapConnection.ScopeSub,
                    options.Filter,
                    options.TargetAttributes,
                    false,
                    searchConstraints
                );

            mappedResultsAccumulator.AddRange(searchResults.Select(converter));

            return searchResults.ResponseControls;
        }
    }
}
