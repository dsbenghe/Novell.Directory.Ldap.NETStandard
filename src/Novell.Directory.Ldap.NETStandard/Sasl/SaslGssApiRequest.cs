#if NET7_0_OR_GREATER
using System;
using System.Net;

namespace Novell.Directory.Ldap.Sasl
{
    public class SaslGssApiRequest : SaslRequest
    {
        /// <summary>
        /// Host part of the service principal name (ldap/host). It has to be the name the
        /// server is registered with in the KDC - for Active Directory that is the FQDN of
        /// the domain controller, not the domain name.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Credentials to use instead of the default ones of the current process
        /// (the Kerberos credential cache on Linux and macOS, the logged on user on Windows).
        /// </summary>
        public NetworkCredential Credential { get; }

        public SaslGssApiRequest(string host)
            : this(host, null)
        {
        }

        public SaslGssApiRequest(string host, NetworkCredential credential)
            : base(SaslConstants.Mechanism.GssApi)
        {
            if (string.IsNullOrEmpty(host))
            {
                throw new ArgumentException("Host must be specified", nameof(host));
            }

            Host = host;
            Credential = credential;
        }
    }
}
#endif
