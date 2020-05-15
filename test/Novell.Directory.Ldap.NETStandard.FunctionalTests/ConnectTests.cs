using System;
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
                }, true, true);
        }

        [Fact]
        public void Connect_WithStartTls_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    try
                    {
                        ldapConnection.StartTls();
                        ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    }
                    finally
                    {
                        //ldapConnection.StopTls();
                    }
                }, false, true);
        }

        [Fact]
        public void Connect_WithStartTlsAfterBindWithNonTls_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StartTls();
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StopTls();
                }, false, true);
        }

        [Fact(Skip = "This randomly fails")]
        public void Connect_WithBindAfterStartTlsAndRestoreNonTls_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StartTls();
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StopTls();
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                }, false, true);
        }

        [Fact]
        public void Connect_WithStartTls_And_Without_StopTls_Works()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                    ldapConnection.StartTls();
                }, false, true);
        }
    }
}
