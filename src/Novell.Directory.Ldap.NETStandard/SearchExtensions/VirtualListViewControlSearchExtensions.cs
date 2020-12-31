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
        public static List<LdapEntry> SearchUsingVlv(
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
                .SearchUsingVlv(sortControl, options, pageSize);
        }

        public static List<T> SearchUsingVlv<T>(
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
                .SearchUsingVlv(sortControl, converter, options, pageSize);
        }
    }
}
