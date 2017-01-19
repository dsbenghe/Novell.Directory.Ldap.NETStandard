using System;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class TestHelper
    {
        public static void WithLdapConnection(Action<ILdapConnection> actionOnConnectedLdapConnection, bool useSsl = false)
        {
            WithLdapConnectionImpl<object>((ldapConnection) =>
            {
                actionOnConnectedLdapConnection(ldapConnection);
                return null;
            }, useSsl);
        }

        public static void WithAuthenticatedLdapConnection(Action<ILdapConnection> actionOnAuthenticatedLdapConnection)
        {
            WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                actionOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        public static T WithLdapConnection<T>(Func<ILdapConnection, T> funcOnConnectedLdapConnection, bool useSsl = false)
        {
            return WithLdapConnectionImpl(funcOnConnectedLdapConnection);
        }

        public static T WithAuthenticatedLdapConnection<T>(Func<ILdapConnection, T> funcOnAuthenticatedLdapConnection)
        {
            return WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                return funcOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        private static T WithLdapConnectionImpl<T>(Func<ILdapConnection, T> funcOnConnectedLdapConnection, bool useSsl = false)
        {
            using (var ldapConnection = new LdapConnection())
            {
                ldapConnection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;
                if (useSsl)
                {                    
                    ldapConnection.SecureSocketLayer = true;
                }
                ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, useSsl ? TestsConfig.LdapServer.ServerPortSsl : TestsConfig.LdapServer.ServerPort);
                return funcOnConnectedLdapConnection(ldapConnection);
            }
        }

        public static string BuildDn(string cn)
        {
            return $"cn={cn}," + TestsConfig.LdapServer.BaseDn;
        }
    }
}