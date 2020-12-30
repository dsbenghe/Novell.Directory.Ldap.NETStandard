using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Novell.Directory.Ldap.Sasl;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public sealed class ConnectWithClientCertificateTests : IDisposable
    {
        private readonly X509Certificate2 _x509Certificate2;
        private readonly LdapConnectionOptions _ldapConnectionOptions;
        private readonly string _expectedAuthzId;

        public ConnectWithClientCertificateTests()
        {
            _expectedAuthzId = "dn:cn=external-test,dc=example,dc=com";
            _x509Certificate2 = new X509Certificate2(
                TestHelper.GetCertificate("external-test.pfx"),
                "password");
            _ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork)
                .ConfigureClientCertificates(new List<X509Certificate>()
                {
                    _x509Certificate2,
                });
        }

        [Fact]
        public void Bind_with_client_certificate_is_successful()
        {
            _ldapConnectionOptions.UseSsl();
            using var ldapConnection = new LdapConnection(_ldapConnectionOptions);
            ldapConnection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;

            ldapConnection.UserDefinedClientCertSelectionDelegate +=
                LdapConnection_UserDefinedClientCertSelectionDelegate;
            ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPortSsl);

            ldapConnection.Bind(new SaslExternalRequest());

            Assert.True(ldapConnection.Bound);
            var response = ldapConnection.WhoAmI();
            Assert.Equal(_expectedAuthzId, response.AuthzId);
        }

        [Fact]
        public void Bind_with_client_certificate_using_start_tls_is_successful()
        {
            using var ldapConnection = new LdapConnection(_ldapConnectionOptions);
            ldapConnection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;

            ldapConnection.UserDefinedClientCertSelectionDelegate +=
                LdapConnection_UserDefinedClientCertSelectionDelegate;
            ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort);

            try
            {
                ldapConnection.StartTls();

                ldapConnection.Bind(new SaslExternalRequest());

                Assert.True(ldapConnection.Bound);
                var response = ldapConnection.WhoAmI();
                Assert.Equal(_expectedAuthzId, response.AuthzId);
            }
            finally
            {
                ldapConnection.StopTls();
            }
        }

        private X509Certificate LdapConnection_UserDefinedClientCertSelectionDelegate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _x509Certificate2;
        }

        public void Dispose()
        {
            _x509Certificate2?.Dispose();
        }
    }
}
