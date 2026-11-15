#if NET7_0_OR_GREATER
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Novell.Directory.Ldap.Sasl;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    // The KDC, the ldap/localhost keytab for slapd and the ticket of gssapi-test are set up by configure-openldap.sh
    public class SaslGssApiTests
    {
        private const string ExpectedAuthzId = "dn:cn=gssapi-test,dc=example,dc=com";

        [Fact]
        public async Task Bind_with_ticket_cache_is_successful()
        {
            await TestHelper.WithLdapConnectionAsync(async ldapConnection =>
            {
                await ldapConnection.BindAsync(new SaslGssApiRequest(TestsConfig.LdapServer.ServerAddress));

                Assert.True(ldapConnection.Bound);
                var response = await ((LdapConnection)ldapConnection).WhoAmIAsync();
                Assert.Equal(ExpectedAuthzId, response.AuthzId);
            });
        }

        [Fact]
        public async Task Bind_with_credential_is_successful()
        {
            await TestHelper.WithLdapConnectionAsync(async ldapConnection =>
            {
                var credential = new NetworkCredential("gssapi-test", "password", "EXAMPLE.COM");
                await ldapConnection.BindAsync(new SaslGssApiRequest(TestsConfig.LdapServer.ServerAddress, credential));

                Assert.True(ldapConnection.Bound);
                var response = await ((LdapConnection)ldapConnection).WhoAmIAsync();
                Assert.Equal(ExpectedAuthzId, response.AuthzId);
            });
        }

        [Fact]
        public async Task Bind_with_unknown_service_principal_fails()
        {
            await TestHelper.WithLdapConnectionAsync(async ldapConnection =>
            {
                await Assert.ThrowsAsync<LdapException>(
                    () => ldapConnection.BindAsync(new SaslGssApiRequest("unknown.example.com")));

                Assert.False(ldapConnection.Bound);
            });
        }
    }
}
#endif
