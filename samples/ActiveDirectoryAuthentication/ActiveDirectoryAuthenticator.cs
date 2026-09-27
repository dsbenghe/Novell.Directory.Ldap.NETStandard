using System;
using System.Threading;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.Samples.ActiveDirectoryAuthentication
{
    /// <summary>Connection settings for <see cref="ActiveDirectoryAuthenticator"/>.</summary>
    public sealed class ActiveDirectoryOptions
    {
        /// <summary>Domain controller host name or IP.</summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>389 for plain/StartTLS, 636 for LDAPS.</summary>
        public int Port { get; set; } = LdapConnection.DefaultPort;

        /// <summary>Use LDAPS (implicit TLS on connect). Mutually exclusive with <see cref="UseStartTls"/>.</summary>
        public bool UseSsl { get; set; }

        /// <summary>Upgrade a plain connection with StartTLS after connecting.</summary>
        public bool UseStartTls { get; set; }

        /// <summary>
        /// Appended to bare user names so <c>jdoe</c> becomes <c>jdoe@example.com</c>.
        /// Leave empty to require callers to pass a full UPN or <c>DOMAIN\user</c>.
        /// </summary>
        public string DefaultUpnSuffix { get; set; } = string.Empty;

        /// <summary>
        /// Upper bound for connect + TLS + bind. Without it an unreachable host blocks for the
        /// OS TCP timeout (often 1–2 minutes on Linux), which is unacceptable on a login page.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Verifies a user's password against Active Directory with a simple bind and
    /// reports <em>why</em> a bind failed, instead of a bare exception.
    /// </summary>
    public sealed class ActiveDirectoryAuthenticator
    {
        private readonly ActiveDirectoryOptions _options;

        public ActiveDirectoryAuthenticator(ActiveDirectoryOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                throw new ArgumentException("Host is required.", nameof(options));
            }

            if (_options.UseSsl && _options.UseStartTls)
            {
                throw new ArgumentException("UseSsl and UseStartTls are mutually exclusive.", nameof(options));
            }
        }

        /// <summary>
        /// Binds as <paramref name="userName"/> and classifies the outcome.
        /// Never throws for authentication failures; only argument errors propagate.
        /// </summary>
        /// <param name="userName">UPN (<c>user@domain</c>), <c>DOMAIN\user</c>, or a bare name if <see cref="ActiveDirectoryOptions.DefaultUpnSuffix"/> is set.</param>
        public async Task<AdBindResult> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                // An empty password would turn into an anonymous bind, which AD accepts.
                return AdBindResult.InvalidCredentials;
            }

            var bindName = QualifyUserName(userName);

            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                timeout.CancelAfter(_options.Timeout);
                var token = timeout.Token;

                try
                {
                var connectionOptions = new LdapConnectionOptions();
                if (_options.UseSsl)
                {
                    connectionOptions.UseSsl();
                }

                using (var connection = new LdapConnection(connectionOptions))
                {
                    await connection.ConnectAsync(_options.Host, _options.Port, token).ConfigureAwait(false);

                    if (_options.UseStartTls)
                    {
                        await connection.StartTlsAsync(token).ConfigureAwait(false);
                    }

                    await connection.BindAsync(bindName, password, token).ConfigureAwait(false);
                    return AdBindResult.Success;
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Our own timeout fired (the caller's token is still live).
                return AdBindResult.ConnectionError;
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.InvalidCredentials)
            {
                return AdBindErrorParser.Parse(ex);
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.ConnectError
                                        || ex.ResultCode == LdapException.ServerDown
                                        || ex.ResultCode == LdapException.LdapTimeout)
            {
                return AdBindResult.ConnectionError;
            }
            catch (LdapException)
            {
                return AdBindResult.Unknown;
            }
            catch (System.Net.Sockets.SocketException)
            {
                return AdBindResult.ConnectionError;
            }
            catch (System.IO.IOException)
            {
                return AdBindResult.ConnectionError;
            }
            }
        }

        private string QualifyUserName(string userName)
        {
            if (userName.Contains("@") || userName.Contains("\\") || string.IsNullOrEmpty(_options.DefaultUpnSuffix))
            {
                return userName;
            }

            var suffix = _options.DefaultUpnSuffix.StartsWith("@", StringComparison.Ordinal)
                ? _options.DefaultUpnSuffix
                : "@" + _options.DefaultUpnSuffix;

            return userName + suffix;
        }
    }
}
