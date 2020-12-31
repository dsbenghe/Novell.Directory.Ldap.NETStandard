using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Sasl.Clients;
using System;
using System.Collections.Generic;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Sasl
{
    public class PlainClientTests
    {
        private const string AuthId = "admin";
        private const string Password = "password";

        private static readonly IEnumerable<byte> ExpectedResponse = new byte[]
        {
            0x00,

            // admin
            0x61, 0x64, 0x6D, 0x69, 0x6E,
            0x00,

            // password
            0x70, 0x61, 0x73, 0x73, 0x77, 0x6F, 0x72, 0x64,
        };

        [Fact]
        public void HasInitialResponse()
        {
            var request = new SaslPlainRequest(AuthId, Password);
            var client = new PlainClient(request);
            Assert.True(client.HasInitialResponse);
        }

        [Fact]
        public void CreatesChallengeProperly()
        {
            var request = new SaslPlainRequest(AuthId, Password);
            var client = new PlainClient(request);
            var result = client.EvaluateChallenge(Array.Empty<byte>());
            Assert.Equal(ExpectedResponse, result);
        }
    }
}
