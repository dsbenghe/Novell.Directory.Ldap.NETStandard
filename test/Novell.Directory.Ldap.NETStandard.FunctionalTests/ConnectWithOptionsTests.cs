using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ConnectWithOptionsTests
    {
        [Fact]
        public async Task Connect_with_os_selected_ssl_protocol_connects()
        {
            var options = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureIpAddressFilter(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true)
                .ConfigureSslProtocols(SslProtocols.None);
            using var ldapConnection = new LdapConnection(options);

            await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPortSsl);
        }

        [Fact]
        public async Task Connect_with_obsolete_ssl_version_throws_on_connect()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var options = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureIpAddressFilter(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true)
                .ConfigureSslProtocols(SslProtocols.Ssl2);
#pragma warning restore CS0618 // Type or member is obsolete
            using var ldapConnection = new LdapConnection(options);

            // exception thrown is different on Windows vs Linux
            var exceptionThrowType = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? typeof(Win32Exception) : typeof(AuthenticationException);

            await Assert.ThrowsAsync(
                exceptionThrowType,
                async () => await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPortSsl));
        }

        [Fact]
        public async Task Connect_with_no_ip_selected_throws()
        {
            var options = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(ip => false);
            using var ldapConnection = new LdapConnection(options);

            await Assert.ThrowsAsync<ArgumentException>(
                "ipAddress",
                async () => await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort));
        }

        [Fact]
        public async Task Connect_with_ipv4_selected_connects()
        {
            var options = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            using var ldapConnection = new LdapConnection(options);

            await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort);

            Assert.True(ldapConnection.Connected);
        }

        [Fact]
        public async Task Connect_with_ipv6_selected_connects()
        {
            var options = new LdapConnectionOptions()
                .ConfigureIpAddressFilter(ip => ip.AddressFamily == AddressFamily.InterNetworkV6);
            using var ldapConnection = new LdapConnection(options);

            await ldapConnection.ConnectAsync(TestsConfig.LdapServer.ServerAddress, TestsConfig.LdapServer.ServerPort);

            Assert.True(ldapConnection.Connected);
        }

        [Fact]
        public async Task Connect_with_CancellationToken()
        {
            using var ldapConnection = new LdapConnection();
            using var cts = new CancellationTokenSource(100);

            ldapConnection.ConnectionTimeout = 200;

            var sw = Stopwatch.StartNew();

            var ex = await Assert.ThrowsAsync<LdapException>(() => ldapConnection.ConnectAsync("1.1.1.1", 636, cts.Token));

            Assert.IsType<OperationCanceledException>(ex.InnerException);

            var em = sw.ElapsedMilliseconds;

            Assert.True(em > 100 && em < 200);
        }
    }
}
