using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public Task<List<LdapEntry>> SearchWithSimplePagingAsync(
            [NotNull] SearchOptions options,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return SearchWithSimplePagingAsync(entry => entry, options, pageSize, cancellationToken);
        }

        public async Task<List<T>> SearchWithSimplePagingAsync<T>(
            [NotNull] Func<LdapEntry, T> converter,
            [NotNull] SearchOptions options,
            int pageSize,
            CancellationToken cancellationToken = default)
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
                var responseControls = await RetrievePageAsync(options, searchConstraints, searchResult, converter, cancellationToken).ConfigureAwait(false);
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

        private async Task<LdapControl[]> RetrievePageAsync<T>(
            [NotNull] SearchOptions options,
            [NotNull] LdapSearchConstraints searchConstraints,
            [NotNull] List<T> mappedResultsAccumulator,
            [NotNull] Func<LdapEntry, T> converter,
            CancellationToken cancellationToken = default)
        {
            if (searchConstraints == null)
            {
                throw new ArgumentNullException(nameof(searchConstraints));
            }

            if (mappedResultsAccumulator == null)
            {
                throw new ArgumentNullException(nameof(mappedResultsAccumulator));
            }

            var asyncSearchResults = await _ldapConnection.SearchAsync(
                    options.SearchBase,
                    LdapConnection.ScopeSub,
                    options.Filter,
                    options.TargetAttributes,
                    false,
                    searchConstraints,
                    cancellationToken
                ).ConfigureAwait(false);

            var searchResults = await asyncSearchResults.ToListAsync(cancellationToken).ConfigureAwait(false);

            mappedResultsAccumulator.AddRange(searchResults.Select(converter));

            return asyncSearchResults.ResponseControls;
        }
    }
}
