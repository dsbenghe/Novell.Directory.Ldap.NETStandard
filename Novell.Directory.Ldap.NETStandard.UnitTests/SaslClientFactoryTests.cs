using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Sasl.Clients;
using System.Collections;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class SaslClientFactoryTests
    {
        private const string _proto = DefaultSaslClientFactory.ProtocolLdap;
        private const string _gibberish = "7c566abfaae049d893df01cc811d3e17";

        [Fact]
        public void CramMd5_CreatesCramMD5Client()
        {
            var client = DefaultSaslClientFactory.CreateClient(SaslConstants.Mechanism.CramMd5, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<CramMD5Client>(client);
            Assert.Equal(SaslConstants.Mechanism.CramMd5, client.MechanismName);
            Assert.False(client.HasInitialResponse);
        }

        [Fact]
        public void Unknown_ReturnsNull()
        {
            var client = DefaultSaslClientFactory.CreateClient(_gibberish, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.Null(client);
        }

        [Fact]
        public void LdapConnection_IsSaslMechanismSupported_CramMd5_True()
        {
            var conn = new LdapConnection();
            Assert.True(conn.IsSaslMechanismSupported(SaslConstants.Mechanism.CramMd5));
        }

        [Fact]
        public void LdapConnection_IsSaslMechanismSupported_Unknown_False()
        {
            var conn = new LdapConnection();
            Assert.False(conn.IsSaslMechanismSupported(_gibberish));
        }

        [Fact]
        public void LdapConnection_GetRegisteredSaslClientFactories_EmptyNotNull()
        {
            var conn = new LdapConnection();
            var factories = conn.GetRegisteredSaslClientFactories();
            Assert.NotNull(factories);
            Assert.Empty(factories);
        }

        [Fact]
        public void LdapConnection_GetRegisteredSaslClientFactories_ReturnsFactories()
        {
            const string mechanism = "TestMechanism";
            var factory = new TestSaslClientFactory(mechanism);

            var conn = new LdapConnection();
            conn.RegisterSaslClientFactory(factory);

            var factories = conn.GetRegisteredSaslClientFactories();
            Assert.NotNull(factories);
            Assert.Equal(1, factories.Count);
            Assert.Equal(mechanism, factories.Single().SupportedMechanisms.Single());
        }

        [Fact]
        public void LdapConnection_CreateClient_Unknown_Null()
        {
            var conn = new LdapConnection();
            var client = conn.CreateClient(_gibberish, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.Null(client);
        }

        [Fact]
        public void LdapConnection_CreateClient_CramMd5_DefaultCramMD5Client()
        {
            var conn = new LdapConnection();
            var client = conn.CreateClient(SaslConstants.Mechanism.CramMd5, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<CramMD5Client>(client);
            Assert.Equal(SaslConstants.Mechanism.CramMd5, client.MechanismName);
            Assert.False(client.HasInitialResponse);
        }

        [Fact]
        public void LdapConnection_RegisterSaslClientFactory_NewMechanism_CreatesClient()
        {
            const string mechanism = "MyNewMechanism";
            var conn = new LdapConnection();
            var client = conn.CreateClient(mechanism, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.Null(client);

            var factory = new TestSaslClientFactory(mechanism);
            conn.RegisterSaslClientFactory(factory);

            client = conn.CreateClient(mechanism, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<TestSaslClient>(client);
        }

        [Fact]
        public void LdapConnection_RegisterSaslClientFactory_CramMd5_OverridesDefaults()
        {
            const string mechanism = SaslConstants.Mechanism.CramMd5;
            var conn = new LdapConnection();
            var client = conn.CreateClient(mechanism, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<CramMD5Client>(client);

            var factory = new TestSaslClientFactory(mechanism);
            conn.RegisterSaslClientFactory(factory);

            client = conn.CreateClient(mechanism, "unused", _proto, "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<TestSaslClient>(client);
        }
    }
}
