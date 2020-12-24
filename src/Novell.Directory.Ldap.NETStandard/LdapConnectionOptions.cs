using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Novell.Directory.Ldap
{
    /// <summary>
    /// Allow tweaking some of the options of connecting to the Ldap server.
    /// </summary>
    public class LdapConnectionOptions
    {
        private List<X509Certificate> _clientCertificates = new List<X509Certificate>();

        public Func<IPAddress, bool> IpAddressFilter { get; private set; } = ipAddress => true;

        public bool Ssl { get; private set; }

        public IReadOnlyCollection<X509Certificate> ClientCertificates
        {
            get
            {
                return _clientCertificates;
            }
        }

        public SslProtocols SslProtocols { get; private set; } = SslProtocols.None;
        public bool CheckCertificateRevocationEnabled { get; private set; }

        /// <summary>
        /// Configure an ip address filter.
        /// By default the first ip address of type <see cref="AddressFamily.InterNetwork"/> or <see cref="AddressFamily.InterNetworkV6"/>
        /// returned by the dns resolving wil be used.
        /// Can be used if for example we want to use exclusively IPV6.
        /// </summary>
        public LdapConnectionOptions UseIpAddressFilter(Func<IPAddress, bool> ipAddressFilter)
        {
            IpAddressFilter = ipAddressFilter ?? throw new ArgumentNullException(nameof(ipAddressFilter));

            return this;
        }

        /// <summary>
        /// Configure to use SSL when connecting to the ldap server.
        /// By default is not.
        /// </summary>
        public LdapConnectionOptions UseSsl()
        {
            Ssl = true;

            return this;
        }

        /// <summary>
        /// Configure the client certificates to be used while establishing the SSL connection.
        /// By default none will be used.
        /// </summary>
        public LdapConnectionOptions UseClientCertificates(IEnumerable<X509Certificate> clientCertificates)
        {
            var clientCertificatesList = clientCertificates?.ToList();
            _clientCertificates = clientCertificatesList ?? throw new ArgumentNullException(nameof(clientCertificatesList));

            return this;
        }

        /// <summary>
        /// Configure the ssl protocols versions to be used.
        /// By default, the OS selected secure options will be used. Equivalent to <see cref="SslProtocols.None"/>.
        /// </summary>
        public LdapConnectionOptions UseSslProtocols(SslProtocols sslProtocols)
        {
            SslProtocols = sslProtocols;

            return this;
        }

        /// <summary>
        /// Configure to check the certificate revocation.
        /// By default will use <see cref="X509RevocationMode.NoCheck"/>.
        /// Setting this will configure to use <see cref="X509RevocationMode.Online"/>.
        /// </summary>
        public LdapConnectionOptions CheckCertificateRevocation()
        {
            CheckCertificateRevocationEnabled = true;

            return this;
        }

        internal void SetSecureSocketLayer(bool ssl)
        {
            Ssl = ssl;
        }
    }
}
