using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Novell.Directory.Ldap.Controls;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    /// Provides utility method to load and map all desired entries via n requests
    /// with <see cref="LdapPagedResultsControl"/>.
    /// </summary>
    public class PagedResultsControlHandler<T>
    {
        private readonly Func<LdapEntry, T> _converter;

        public PagedResultsControlHandler([NotNull] Func<LdapEntry, T> converter)
        {
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public async Task<List<T>> LoadAllPagedResults([NotNull] SearchOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            using (var ldapConnection = await ConnectAsync(options))
            {
                var searchResult = new List<T>();
                var isNextPageAvailable = PrepareForNextPage(ldapConnection, null, options.ResultPageSize, true);
                while (isNextPageAvailable)
                {
                    var responseControls = await RetrievePageAsync(ldapConnection, options, searchResult);
                    isNextPageAvailable = PrepareForNextPage(ldapConnection, responseControls, options.ResultPageSize, false);
                }

                return searchResult;
            }
        }

        private static async Task<LdapConnection> ConnectAsync([NotNull] SearchOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            var ldapConnection = new LdapConnection {SecureSocketLayer = false};
            await ldapConnection.ConnectAsync(options.Host, options.Port);
            await ldapConnection.BindAsync(options.ProtocolVersion, options.Login, options.Password);
            Debug.WriteLine($@"Connected to <{options.Host}:{options.Port}> as <{options.Login}>");
            return ldapConnection;
        }

        private static bool PrepareForNextPage([NotNull] LdapConnection ldapConnection, [CanBeNull] LdapControl[] pageResponseControls, int pageSize,
            bool isInitialCall)
        {
            if (ldapConnection == null) throw new ArgumentNullException(nameof(ldapConnection));
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

            var cookie = LdapPagedResultsControl.GetEmptyCookie;
            if (!isInitialCall)
            {
                var pagedResultsControl = (LdapPagedResultsControl) pageResponseControls?.FirstOrDefault(x => x is LdapPagedResultsControl);
                if (pagedResultsControl == null)
                {
                    Debug.WriteLine($"Failed to find <{nameof(LdapPagedResultsControl)}>. Searching is abruptly stopped");
                    return false;
                }

                // server signaled end of result set
                if (pagedResultsControl.IsEmptyCookie()) return false;
                cookie = pagedResultsControl.Cookie;
            }

            ApplyPagedResultsControl(ldapConnection, pageSize, cookie);
            return true;
        }

        private static void ApplyPagedResultsControl([NotNull] LdapConnection connection, int pageSize, [CanBeNull] byte[] cookie)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var ldapPagedControl = new LdapPagedResultsControl(pageSize, cookie);
            var searchConstraints = connection.SearchConstraints;
            searchConstraints.BatchSize = 0;
            searchConstraints.SetControls(ldapPagedControl);
            connection.Constraints = searchConstraints;
        }

        private async Task<LdapControl[]> RetrievePageAsync([NotNull] LdapConnection ldapConnection, [NotNull] SearchOptions options,
            [NotNull] List<T> mappedResultsAccumulator)
        {
            if (ldapConnection == null) throw new ArgumentNullException(nameof(ldapConnection));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (mappedResultsAccumulator == null) throw new ArgumentNullException(nameof(mappedResultsAccumulator));

            var searchResults = await ldapConnection.SearchAsync
            (
                options.SearchBase,
                LdapConnection.ScopeSub,
                options.Filter,
                options.TargetAttributes,
                false,
                (LdapSearchConstraints) null
            );

            while (searchResults.HasMore())
            {
                try
                {
                    var nextEntry = searchResults.Next();
                    var mappedEntry = _converter.Invoke(nextEntry);
                    mappedResultsAccumulator.Add(mappedEntry);
                }
                catch (LdapException ex)
                {
                    // you may want to turn referral chasing on
                    if (ex is LdapReferralException) continue;
                    throw new InvalidOperationException("Failed to proceed to the next search result", ex);
                }
            }

            return ((LdapSearchResults) searchResults).ResponseControls;
        }
    }
}