using System.Threading.Tasks;
using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ConnectTests
    {
        [Fact]
        public async Task Connect_when_correct_credentials_is_successful()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                });
        }

        [Fact]
        public async Task Connect_when_empty_dn_throws_exception()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await Assert.ThrowsAsync<LdapException>(
                        async () => await ldapConnection.BindAsync(
                            string.Empty, 
                            TestsConfig.LdapServer.RootUserPassword));
                });
        }

        [Fact]
        public async Task Connect_when_wrong_password_throws_exception()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await Assert.ThrowsAsync<LdapException>(
                        async () => await ldapConnection.BindAsync(
                            TestsConfig.LdapServer.RootUserDn, 
                            TestsConfig.LdapServer.RootUserPassword + "1"));
                });
        }

        [Fact]
        public async Task  Connect_WithSsl_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                }, true, true);
        }

        [Fact]
        public async Task Connect_WithStartTls_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    try
                    {
                        await ldapConnection.StartTlsAsync();
                        await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    }
                    finally
                    {
                        await ldapConnection.StopTlsAsync();
                    }
                }, false, true);
        }

        [Fact]
        public async Task Disconnect_WithStartTls_WithoutStopTls_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.StartTlsAsync();
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                }, false, true);
        }

        [Fact]
        public async Task  Connect_WithStartTlsAfterBindWithNonTls_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.StartTlsAsync();
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.StopTlsAsync();
                }, false, true);
        }

        [Fact]
        public async Task Connect_WithBindAfterStartTlsAndRestoreNonTls_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.StartTlsAsync();
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.StopTlsAsync();
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                }, false, true);
        }

        [Fact]
        public async Task Connect_WithStartTls_And_Without_StopTls_Works()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    await ldapConnection.StartTlsAsync();
                }, false, true);
        }
    }
}
