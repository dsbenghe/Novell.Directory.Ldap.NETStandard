using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;
using System;
using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// Provides extensions methods for <see cref="ILdapConnection"/> to do paged searches
    /// with <see cref="SimplePagedResultsControl"/> using <see cref="SimplePagedResultsControlHandler"/>.
    /// </summary>
    public static class SimplePagedResultsControlSearchExtensions
    {
        public static List<LdapEntry> SearchUsingSimplePaging([NotNull] this ILdapConnection ldapConnection, [NotNull] SearchOptions options, int pageSize)
        {
            if (ldapConnection == null)
            {
                throw new ArgumentNullException(nameof(ldapConnection));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            return new SimplePagedResultsControlHandler(ldapConnection)
                .SearchWithSimplePaging(options, pageSize);
        }

        public static List<T> SearchUsingSimplePaging<T>([NotNull] this ILdapConnection ldapConnection, [NotNull] Func<LdapEntry, T> converter, [NotNull] SearchOptions options, int pageSize)
        {
            if (ldapConnection == null)
            {
                throw new ArgumentNullException(nameof(ldapConnection));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new SimplePagedResultsControlHandler(ldapConnection)
                .SearchWithSimplePaging(converter, options, pageSize);
        }
    }
}
