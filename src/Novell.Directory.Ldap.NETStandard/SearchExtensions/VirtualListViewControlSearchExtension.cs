using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap.SearchExtensions
{
    public static class VirtualListViewControlSearchExtension
    {
        public static List<LdapEntry> SearchUsingVlv(
            [NotNull] this ILdapConnection ldapConnection,
            [NotNull] LdapSortControl sortControl,
            [NotNull] SearchOptions options, 
            int pageSize)
        {
            if (ldapConnection == null) throw new ArgumentNullException(nameof(ldapConnection));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            return ldapConnection.SearchUsingVlv(sortControl, entry => entry, options, pageSize);
        }

        public static List<T> SearchUsingVlv<T>(
            [NotNull] this ILdapConnection ldapConnection,
            [NotNull] LdapSortControl sortControl,
            [NotNull] Func<LdapEntry, T> converter,
            [NotNull] SearchOptions options, 
            int pageSize)
        {
            var entries = new List<T>();
            var pageCount = 1;
            var searchConstraints = options.SearchConstraints ?? ldapConnection.SearchConstraints;

            while (true)
            {
                searchConstraints.SetControls(new LdapControl[]
                {
                    BuildLdapVirtualListControl(pageCount, pageSize),
                    sortControl
                });

                var searchResults = ldapConnection.Search(
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
            var startIndex = (page - 1) * pageSize + 1;
            var beforeCount = 0;
            var afterCount = pageSize - 1;
            var contentCount = 0;

            return new LdapVirtualListControl(startIndex, beforeCount, afterCount, contentCount);
        }
    }
}
