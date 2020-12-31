using Microsoft.Extensions.DependencyModel;
using System;
using System.Net.Sockets;
using System.Reflection;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class TestHelper
    {
        private static TransportSecurity? envTransportSecurity = null;

        private enum TransportSecurity
        {
            Off,
            Ssl,
            Tls,
        }

        public static void WithLdapConnection(Action<ILdapConnection> actionOnConnectedLdapConnection, bool useSsl = false, bool disableEnvTransportSecurity = false)
        {
            WithLdapConnectionImpl<object>(
                ldapConnection =>
            {
                actionOnConnectedLdapConnection(ldapConnection);
                return null;
            }, useSsl, disableEnvTransportSecurity);
        }

        public static void WithAuthenticatedLdapConnection(Action<ILdapConnection> actionOnAuthenticatedLdapConnection)
        {
            WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                actionOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        public static T WithAuthenticatedLdapConnection<T>(Func<ILdapConnection, T> funcOnAuthenticatedLdapConnection)
        {
            return WithLdapConnection(ldapConnection =>
            {
                ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                return funcOnAuthenticatedLdapConnection(ldapConnection);
            });
        }

        private static T WithLdapConnection<T>(Func<ILdapConnection, T> funcOnConnectedLdapConnection)
        {
            return WithLdapConnectionImpl(funcOnConnectedLdapConnection);
        }

        private static T WithLdapConnectionImpl<T>(Func<ILdapConnection, T> funcOnConnectedLdapConnection, bool useSsl = false, bool disableEnvTransportSecurity = false)
        {
            var ldapConnectionOptions = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(ipAddress => ipAddress.AddressFamily == AddressFamily.InterNetwork)
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);
            using (var ldapConnection = new LdapConnection(ldapConnectionOptions))
            {
                var ldapPort = TestsConfig.LdapServer.ServerPort;
                var transportSecurity = GetTransportSecurity(useSsl, disableEnvTransportSecurity);
                if (transportSecurity == TransportSecurity.Ssl)
                {
                    ldapConnection.SecureSocketLayer = true;
                    ldapPort = TestsConfig.LdapServer.ServerPortSsl;
                }

                ldapConnection.Connect(TestsConfig.LdapServer.ServerAddress, ldapPort);

                T retValue;
                if (transportSecurity == TransportSecurity.Tls)
                {
                    try
                    {
                        ldapConnection.StartTls();
                        retValue = funcOnConnectedLdapConnection(ldapConnection);
                    }
                    finally
                    {
                        ldapConnection.StopTls();
                    }
                }
                else
                {
                    retValue = funcOnConnectedLdapConnection(ldapConnection);
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

            transportSecurity = GetTransportSecurity(transportSecurity);

            return transportSecurity;
        }

        private static TransportSecurity GetTransportSecurity(TransportSecurity transportSecurity)
        {
            if (envTransportSecurity.HasValue)
            {
                return transportSecurity;
            }

            var envValue = Environment.GetEnvironmentVariable("TRANSPORT_SECURITY");
            if (string.IsNullOrWhiteSpace(envValue))
            {
                return transportSecurity;
            }

            if (!Enum.TryParse(envValue, true, out TransportSecurity parsedEnvTransportSecurity))
            {
                return transportSecurity;
            }

            envTransportSecurity = parsedEnvTransportSecurity;
            Console.WriteLine($"Using env variable for transport security {envTransportSecurity}");
            transportSecurity = envTransportSecurity.Value;

            return transportSecurity;
        }

        public static string BuildDn(string cn)
        {
            return $"cn={cn}," + TestsConfig.LdapServer.BaseDn;
        }

        public static byte[] GetCertificate(string name)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var manifestResourceStream = executingAssembly.GetManifestResourceStream($"{executingAssembly.GetName().Name}.certs.{name}");

            if (manifestResourceStream == null)
            {
                throw new ArgumentNullException(nameof(manifestResourceStream));
            }

            var certBytes = new byte[manifestResourceStream.Length];
            manifestResourceStream.Read(certBytes, 0, certBytes.Length);

            return certBytes;
        }
    }
}
