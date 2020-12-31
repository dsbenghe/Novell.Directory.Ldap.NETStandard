using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class LdapConnectionOptionsTests
    {
        private static readonly Random RandomGen = new Random();

        [Fact]
        public void New_instance_has_expected_defaults()
        {
            var ldapConnectionOptions = new LdapConnectionOptions();

            Assert.False(ldapConnectionOptions.Ssl);

            var ipAddressV4 = CreateRandomIpAddressV4();
            Assert.True(ldapConnectionOptions.IpAddressFilter(ipAddressV4));

            var ipAddressV6 = CreateRandomIpAddressV6();
            Assert.True(ldapConnectionOptions.IpAddressFilter(ipAddressV6));

            Assert.Empty(ldapConnectionOptions.ClientCertificates);
            Assert.False(ldapConnectionOptions.CheckCertificateRevocationEnabled);

            Assert.Equal(SslProtocols.None, ldapConnectionOptions.SslProtocols);
        }

        [Fact]
        public void UseSsl_enables_ssl()
        {
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl();

            Assert.True(ldapConnectionOptions.Ssl);
        }

        [Fact]
        public void UseSslProtocols_enables_specific_ssl_protocols()
        {
            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureSslProtocols(SslProtocols.Tls11 | SslProtocols.Tls12);

            Assert.True(ldapConnectionOptions.SslProtocols.HasFlag(SslProtocols.Tls11));
            Assert.True(ldapConnectionOptions.SslProtocols.HasFlag(SslProtocols.Tls12));
            Assert.False(ldapConnectionOptions.SslProtocols.HasFlag(SslProtocols.Tls));
            Assert.False(ldapConnectionOptions.SslProtocols.HasFlag(SslProtocols.Tls13));
        }

        [Fact]
        public void UseClientCertificates_stores_certificates()
        {
            var clientCertificates = new List<X509Certificate>()
            {
                new X509Certificate(),
            };
            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureClientCertificates(clientCertificates);

            Assert.Equal(clientCertificates, ldapConnectionOptions.ClientCertificates);
        }

        [Fact]
        public void UseIpAddressFilter_stores_always_false_filter()
        {
            bool AlwaysFalseIpAddressFilter(IPAddress ipAddress) => false;

            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(AlwaysFalseIpAddressFilter);

            var ipAddressV4 = CreateRandomIpAddressV4();
            Assert.Equal(AlwaysFalseIpAddressFilter(ipAddressV4), ldapConnectionOptions.IpAddressFilter(ipAddressV4));
            var ipAddressV6 = CreateRandomIpAddressV6();
            Assert.Equal(AlwaysFalseIpAddressFilter(ipAddressV6), ldapConnectionOptions.IpAddressFilter(ipAddressV6));
        }

        [Fact]
        public void UseIpAddressFilter_filters_specific_ip_address()
        {
            var ipAddressV4 = CreateRandomIpAddressV4();
            bool IpAddressFilter(IPAddress ipAddress) => ipAddressV4.Equals(ipAddress);

            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(IpAddressFilter);

            Assert.True(ldapConnectionOptions.IpAddressFilter(ipAddressV4));
        }

        [Fact]
        public void UseIpAddressFilter_filters_out_specific_ip_address()
        {
            var ipAddressV4 = CreateRandomIpAddressV4();
            var diffIpAddressV4 = CreateRandomIpAddressV4();
            bool IpAddressFilter(IPAddress ipAddress) => ipAddressV4.Equals(ipAddress);

            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(IpAddressFilter);

            Assert.False(ldapConnectionOptions.IpAddressFilter(diffIpAddressV4));
        }

        [Fact]
        public void CheckCertificateRevocation_enables_check_revocation()
        {
            var ldapConnectionOptions = new LdapConnectionOptions()
                .CheckCertificateRevocation();

            Assert.True(ldapConnectionOptions.CheckCertificateRevocationEnabled);
        }

        private static IPAddress CreateRandomIpAddressV6()
        {
            var randomIpAddressV6Bytes = new byte[16];
            RandomGen.NextBytes(randomIpAddressV6Bytes);
            var ipAddressV6 = new IPAddress(randomIpAddressV6Bytes);
            return ipAddressV6;
        }

        private static IPAddress CreateRandomIpAddressV4()
        {
            var randomIpAddressV4Bytes = new byte[4];
            RandomGen.NextBytes(randomIpAddressV4Bytes);
            var ipAddressV4 = new IPAddress(randomIpAddressV4Bytes);
            return ipAddressV4;
        }
    }
}
