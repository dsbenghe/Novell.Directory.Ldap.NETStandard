using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Sasl.Clients;
using System;
using System.Collections.Generic;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Sasl
{
    public class CramMD5ClientTests
    {
        private const string AuthId = "uid=diradmin,cn=users,dc=macds,dc=local";
        private const string Password = "Password1!";

        private static readonly byte[] Challenge
            =
            {
                0x3c, 0x38, 0x38, 0x32, 0x34, 0x38, 0x38, 0x35,
                0x31, 0x30, 0x2e, 0x35, 0x38, 0x33, 0x36, 0x38,
                0x39, 0x39, 0x40, 0x6d, 0x61, 0x63, 0x64, 0x73,
                0x2e, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x3e,
            };

        private static readonly byte[] ExpectedResponse
            =
            {
                0x75, 0x69, 0x64, 0x3d, 0x64, 0x69, 0x72, 0x61, 0x64, 0x6d, 0x69, 0x6e,
                0x2c, 0x63, 0x6e, 0x3d, 0x75, 0x73, 0x65, 0x72, 0x73, 0x2c, 0x64, 0x63,
                0x3d, 0x6d, 0x61, 0x63, 0x64, 0x73, 0x2c, 0x64, 0x63, 0x3d, 0x6c, 0x6f,
                0x63, 0x61, 0x6c, 0x20, 0x63, 0x36, 0x37, 0x36, 0x64, 0x34, 0x37, 0x37,
                0x33, 0x35, 0x34, 0x32, 0x33, 0x63, 0x38, 0x31, 0x39, 0x37, 0x64, 0x63,
                0x33, 0x37, 0x35, 0x33, 0x30, 0x30, 0x66, 0x38, 0x31, 0x65, 0x39, 0x62,
            };

        [Fact]
        public void CreateClient_NullOrEmptyAuthorizationId_Throws()
        {
            Assert.Throws<SaslException>(() =>
            {
                var client = new CramMD5Client(new SaslCramMd5Request(null, Password));
            });

            Assert.Throws<SaslException>(() =>
            {
                var client = new CramMD5Client(new SaslCramMd5Request(string.Empty, Password));
            });
        }

        [Fact]
        public void CreateClient_NullOrEmptyPassword_Throws()
        {
            Assert.Throws<SaslException>(() =>
            {
                var client = new CramMD5Client(new SaslCramMd5Request(AuthId, null));
            });

            Assert.Throws<SaslException>(() =>
            {
                var client = new CramMD5Client(new SaslCramMd5Request(AuthId, string.Empty));
            });
        }

        [Fact]
        public void EvaluateChallenge_Success()
        {
            var client = new CramMD5Client(new SaslCramMd5Request(AuthId, Password));
            Assert.False(client.IsComplete);

            // Step 1: State.Initial => State.CramMd5ResponseSent
            var response = client.EvaluateChallenge(Challenge);
            Assert.False(client.IsComplete);
            Assert.Equal((IEnumerable<byte>)ExpectedResponse, response);

            // Step 2: State.CramMd5ResponseSent => State.ValidServerResponse
            client.EvaluateChallenge(Array.Empty<byte>());
            Assert.True(client.IsComplete);

            // Step 3: State.ValidServerResponse => Exception
            Assert.Throws<SaslException>(() => client.EvaluateChallenge(Array.Empty<byte>()));
        }

        [Fact]
        public void EvaluateChallenge_NonEmptyServerResponse_Exception()
        {
            var client = new CramMD5Client(new SaslCramMd5Request(AuthId, Password));
            Assert.False(client.IsComplete);

            // Step 1: State.Initial => State.CramMd5ResponseSent
            var response = client.EvaluateChallenge(Challenge);
            Assert.False(client.IsComplete);
            Assert.Equal((IEnumerable<byte>)ExpectedResponse, response);

            // Step 2: State.CramMd5ResponseSent => State.InvalidServerResponse
            Assert.Throws<SaslException>(() => client.EvaluateChallenge(new byte[] { 0x00 }));
            Assert.True(client.IsComplete);
        }

        [Fact]
        public void EvaluateChallenge_Disposed_Exception()
        {
            var client = new CramMD5Client(new SaslCramMd5Request(AuthId, Password));
            Assert.False(client.IsComplete);
            client.Dispose();
            Assert.True(client.IsComplete);
            Assert.Throws<SaslException>(() => client.EvaluateChallenge(Challenge));
            Assert.True(client.IsComplete);
        }
    }
}
