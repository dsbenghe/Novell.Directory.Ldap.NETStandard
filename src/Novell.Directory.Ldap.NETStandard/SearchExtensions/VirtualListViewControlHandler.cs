using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Novell.Directory.Ldap.SearchExtensions
{
    /// <summary>
    /// Helper class to searches using <see cref="LdapVirtualListControl"/>.
    /// </summary>
    public class VirtualListViewControlHandler
    {
        [NotNull]
        private readonly ILdapConnection _ldapConnection;

        public VirtualListViewControlHandler([NotNull] ILdapConnection ldapConnection)
        {
            _ldapConnection = ldapConnection ?? throw new ArgumentNullException(nameof(ldapConnection));
        }

        public List<LdapEntry> SearchUsingVlv(
            [NotNull] LdapSortControl sortControl,
            [NotNull] SearchOptions options,
            int pageSize)
        {
            if (sortControl == null)
            {
                throw new ArgumentNullException(nameof(sortControl));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return SearchUsingVlv(sortControl, entry => entry, options, pageSize);
        }

        public List<T> SearchUsingVlv<T>(
            [NotNull] LdapSortControl sortControl,
            [NotNull] Func<LdapEntry, T> converter,
            [NotNull] SearchOptions options,
            int pageSize)
        {
            if (sortControl == null)
            {
                throw new ArgumentNullException(nameof(sortControl));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            var entries = new List<T>();
            var pageCount = 1;
            var searchConstraints = options.SearchConstraints ?? _ldapConnection.SearchConstraints;

            while (true)
            {
                searchConstraints.SetControls(new LdapControl[]
                {
                    BuildLdapVirtualListControl(pageCount, pageSize),
                    sortControl,
                });

                var searchResults = _ldapConnection.Search(
                    options.SearchBase,
                    LdapConnection.ScopeSub,
                    options.Filter,
                    options.TargetAttributes,
                    options.TypesOnly,
                    searchConstraints).ToList();

                entries.AddRange(searchResults.Select(converter));

                if (searchResults.Count < pageSize)
                {
                    break;
                }

                pageCount++;
            }

            return entries;
        }

        private static LdapVirtualListControl BuildLdapVirtualListControl(int page, int pageSize)
        {
            var startIndex = ((page - 1) * pageSize) + 1;
            var beforeCount = 0;
            var afterCount = pageSize - 1;
            var contentCount = 0;

            return new LdapVirtualListControl(startIndex, beforeCount, afterCount, contentCount);
        }
    }
}
