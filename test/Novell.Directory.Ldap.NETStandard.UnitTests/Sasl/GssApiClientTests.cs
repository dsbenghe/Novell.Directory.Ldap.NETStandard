#if NET7_0_OR_GREATER
using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Sasl.Clients;
using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Sasl
{
    public class GssApiClientTests
    {
        private const string Host = "dc1.example.com";

        [Fact]
        public void HasInitialResponse()
        {
            var client = new GssApiClient(new SaslGssApiRequest(Host));
            Assert.True(client.HasInitialResponse);
            Assert.False(client.IsComplete);
            Assert.Equal(SaslConstants.Mechanism.GssApi, client.MechanismName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Request_WithoutHost_Throws(string host)
        {
            Assert.Throws<ArgumentException>(() => new SaslGssApiRequest(host));
        }

        [Fact]
        public void OtherRequestType_Throws()
        {
            Assert.Throws<ArgumentException>(() => new GssApiClient(new SaslPlainRequest("user", "password")));
        }

        [Fact]
        public void EvaluateChallenge_AfterDispose_Throws()
        {
            var client = new GssApiClient(new SaslGssApiRequest(Host));
            client.Dispose();
            Assert.True(client.IsComplete);
            Assert.Throws<SaslException>(() => client.EvaluateChallenge(Array.Empty<byte>()));
        }
    }
}
#endif
