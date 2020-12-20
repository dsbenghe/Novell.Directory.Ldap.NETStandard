using System;
using System.Threading.Tasks;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class TestHelper
    {
        private enum TransportSecurity
        {
            Off,
            Ssl,
            Tls,
        }

        public static async Task WithLdapConnectionAsync(Func<ILdapConnection, Task> actionOnConnectedLdapConnection, bool useSsl = false, bool disableEnvTransportSecurity = false)
        {
            await WithLdapConnectionImplAsync<object>(
                async ldapConnection =>
            {
                await actionOnConnectedLdapConnection(ldapConnection);
                return null;
            }, useSsl, disableEnvTransportSecurity);
        }

        public static async Task WithAuthenticatedLdapConnectionAsync(Func<ILdapConnection, Task> actionOnAuthenticatedLdapConnection)
        {
            await WithLdapConnectionAsync(async ldapConnection =>
            {
                await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                await actionOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        private static async Task<T> WithLdapConnectionAsync<T>(Func<ILdapConnection, Task<T>> funcOnConnectedLdapConnection)
        {
            return await WithLdapConnectionImplAsync(funcOnConnectedLdapConnection);
        }

        public static async Task<T> WithAuthenticatedLdapConnectionAsync<T>(Func<ILdapConnection, Task<T>> funcOnAuthenticatedLdapConnection)
        {
            return await WithLdapConnectionAsync(async ldapConnection =>
            {
                await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                return await funcOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        private static async Task<T> WithLdapConnectionImplAsync<T>(Func<ILdapConnection, Task<T>> funcOnConnectedLdapConnection, bool useSsl = false, bool disableEnvTransportSecurity = false)
        {
            using (var ldapConnection = new LdapConnection())
            {
                ldapConnection.UserDefinedServerCertValidationDelegate += (sender, certificate, chain, errors) => true;
                var ldapPort = TestsConfig.LdapServer.ServerPort;
                var transportSecurity = GetTransportSecurity(useSsl, disableEnvTransportSecurity);
                if (transportSecurity == TransportSecurity.Ssl)
                {
                    ldapConnection.SecureSocketLayer = true;
                    ldapPort = TestsConfig.LdapServer.ServerPortSsl;
                }

                await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, ldapPort);

                T retValue;
                if (transportSecurity == TransportSecurity.Tls)
                {
                    try
                    {
                        await ldapConnection.StartTlsAsync();
                        retValue = await funcOnConnectedLdapConnection(ldapConnection);
                    }
                    finally
                    {
                        await ldapConnection.StopTlsAsync();
                    }
                }
                else
                {
                    retValue = await funcOnConnectedLdapConnection(ldapConnection);
                }

                return retValue;
            }
        }

        private static TransportSecurity GetTransportSecurity(bool useSsl, bool disableEnvTransportSecurity)
        {
            var transportSecurity = useSsl ? TransportSecurity.Ssl : TransportSecurity.Off;
            if (disableEnvTransportSecurity)
            {
                return transportSecurity;
            }

            var envValue = Environment.GetEnvironmentVariable("TRANSPORT_SECURITY");
            if (!string.IsNullOrWhiteSpace(envValue))
            {
                if (Enum.TryParse(envValue, true, out TransportSecurity parsedValue))
                {
                    transportSecurity = parsedValue;
                }
            }

            return transportSecurity;
        }

        public static string BuildDn(string cn)
        {
            return $"cn={cn}," + TestsConfig.LdapServer.BaseDn;
        }
    }
}
