using Novell.Directory.Ldap.Sasl;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Sasl
{
    public class SaslBindTests
    {
        private const string Mechanism = "FAILING-TEST";

        [Fact]
        public async Task BindAsync_ClientFailsBeforeFirstRequest_ConnectionCanBeDisposed()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                var port = ((IPEndPoint)listener.LocalEndpoint).Port;
                var accept = listener.AcceptTcpClientAsync();

                var connection = new LdapConnection();
                connection.RegisterSaslClientFactory(new FailingSaslClientFactory());
                await connection.ConnectAsync(IPAddress.Loopback.ToString(), port);
                using var server = await accept;

                await Assert.ThrowsAsync<LdapException>(() => connection.BindAsync(new FailingSaslRequest()));

                // Used to block forever as the bind semaphore was never released.
                var dispose = Task.Run(() => connection.Dispose());
                Assert.Same(dispose, await Task.WhenAny(dispose, Task.Delay(TimeSpan.FromSeconds(10))));
            }
            finally
            {
                listener.Stop();
            }
        }

        private class FailingSaslRequest : SaslRequest
        {
            public FailingSaslRequest()
                : base(Mechanism)
            {
            }
        }

        private class FailingSaslClientFactory : ISaslClientFactory
        {
            public IReadOnlyList<string> SupportedMechanisms { get; } = new[] { Mechanism };

            public ISaslClient CreateClient(SaslRequest saslRequest) => new FailingSaslClient();
        }

        private sealed class FailingSaslClient : ISaslClient
        {
            public DebugId DebugId { get; } = DebugId.ForType<FailingSaslClient>();

            public string MechanismName => Mechanism;

            public bool HasInitialResponse => true;

            public bool IsComplete => false;

            public byte[] EvaluateChallenge(byte[] challenge) => throw new SaslException("No credentials");

            public void Dispose()
            {
            }
        }
    }
}
