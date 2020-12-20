using System.Threading.Tasks;
using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;

namespace Novell.Directory.Ldap.SearchExtensions
{
    /// <summary>
    /// Extensions methods to <see cref="ILdapConnection"/> to be able to do searches using <see cref="LdapVirtualListControl"/>
    /// using <see cref="VirtualListViewControlHandler"/>.
    /// </summary>
    public static class VirtualListViewControlSearchExtensions
    {
        public static Task<List<LdapEntry>> SearchUsingVlvAsync(
            [NotNull] this ILdapConnection ldapConnection,
            [NotNull] LdapSortControl sortControl,
            [NotNull] SearchOptions options,
            int pageSize)
        {
            if (ldapConnection == null)
            {
                throw new ArgumentNullException(nameof(ldapConnection));
            }

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

            return new VirtualListViewControlHandler(ldapConnection)
                .SearchUsingVlvAsync(sortControl, options, pageSize);
        }

        public static Task<List<T>> SearchUsingVlvAsync<T>(
            [NotNull] this ILdapConnection ldapConnection,
            [NotNull] LdapSortControl sortControl,
            [NotNull] Func<LdapEntry, T> converter,
            [NotNull] SearchOptions options,
            int pageSize)
        {
            if (ldapConnection == null)
            {
                throw new ArgumentNullException(nameof(ldapConnection));
            }

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

            return new VirtualListViewControlHandler(ldapConnection)
                .SearchUsingVlvAsync<T>(sortControl, converter, options, pageSize);
        }
    }
}
