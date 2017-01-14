using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ConnectTests
    {
        [Fact]
        public void Connect_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                });
        }

        [Fact]
        public void Connect_WithSsl_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                }, true);
        }

        [Fact]
        public void Connect_WithStartTls_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StartTls();
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StopTls();
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                });
        }
    }
}
