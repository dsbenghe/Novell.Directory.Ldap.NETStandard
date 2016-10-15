using System;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class TestHelper
    {
        public static void WithLdapConnection(Action<ILdapConnection> actionOnConnectedLdapConnection)
        {
            using (var ldapConnection = new LdapConnection())
            {
                ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort);
                actionOnConnectedLdapConnection(ldapConnection);
            }
        }

        public static void WithAuthenticatedLdapConnection(Action<ILdapConnection> actionOnAuthenticatedLdapConnection)
        {
            WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                actionOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        public static T WithLdapConnection<T>(Func<ILdapConnection, T> funcOnConnectedLdapConnection)
        {
            using (var ldapConnection = new LdapConnection())
            {
                ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort);
                return funcOnConnectedLdapConnection(ldapConnection);
            }
        }

        public static T WithAuthenticatedLdapConnection<T>(Func<ILdapConnection, T> funcOnAuthenticatedLdapConnection)
        {
            return WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                return funcOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        public static string BuildDn(string cn)
        {
            return $"cn={cn}," + TestsConfig.LdapServer.BaseDn;
        }
    }
}