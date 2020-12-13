using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// Provides extensions method to do paged searches
    /// with <see cref="SimplePagedResultsControl"/>.
    /// </summary>
    public static class SimplePagedResultsControlHandler
    {
        public static List<LdapEntry> SearchWithSimplePaging([NotNull] this ILdapConnection ldapConnection, [NotNull] SearchOptions options, int pageSize)
        {
            if (ldapConnection == null) throw new ArgumentNullException(nameof(ldapConnection));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            return ldapConnection.SearchWithSimplePaging(entry => entry, options, pageSize);
        }

        public static List<T> SearchWithSimplePaging<T>([NotNull] this ILdapConnection ldapConnection, [NotNull] Func<LdapEntry, T> converter, [NotNull] SearchOptions options, int pageSize)
        {
            if (ldapConnection == null) throw new ArgumentNullException(nameof(ldapConnection));
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var searchResult = new List<T>();
            var searchConstraints = options.SearchConstraints ?? ldapConnection.SearchConstraints;
            var isNextPageAvailable = PrepareForNextPage(ldapConnection, null, pageSize, true, ref searchConstraints);
            while (isNextPageAvailable)
            {
                var responseControls = RetrievePage(ldapConnection, options, searchConstraints, searchResult, converter);
                isNextPageAvailable = PrepareForNextPage(ldapConnection, responseControls, pageSize, false, ref searchConstraints);
            }

            return searchResult;
        }

        private static bool PrepareForNextPage(
            [NotNull] ILdapConnection ldapConnection,
            [CanBeNull] LdapControl[] pageResponseControls, 
            int pageSize,
            bool isInitialCall,
            ref LdapSearchConstraints searchConstraints)
        {
            var cookie = SimplePagedResultsControl.GetEmptyCookie;
            if (!isInitialCall)
            {
                var pagedResultsControl = (SimplePagedResultsControl) pageResponseControls?.SingleOrDefault(x => x is SimplePagedResultsControl);
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

        private static LdapControl[] RetrievePage<T>(
            [NotNull] ILdapConnection ldapConnection, 
            [NotNull] SearchOptions options,
            [NotNull] LdapSearchConstraints searchConstraints,
            [NotNull] List<T> mappedResultsAccumulator,
            [NotNull] Func<LdapEntry, T> converter)
        {
            if (searchConstraints == null) throw new ArgumentNullException(nameof(searchConstraints));
            if (mappedResultsAccumulator == null) throw new ArgumentNullException(nameof(mappedResultsAccumulator));

            var searchResults = ldapConnection.Search(
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