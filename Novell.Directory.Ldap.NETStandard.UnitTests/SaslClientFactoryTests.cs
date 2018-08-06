using Novell.Directory.Ldap.Sasl;
using Novell.Directory.Ldap.Sasl.Clients;
using System.Collections;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class SaslClientFactoryTests
    {
        [Fact]
        public void CramMd5_CreatesCramMD5Client()
        {
            var client = SaslClientFactory.CreateLdapClient(SaslConstants.Mechanism.CramMd5, "unused", "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.NotNull(client);
            Assert.IsType<CramMD5Client>(client);
            Assert.Equal(SaslConstants.Mechanism.CramMd5, client.MechanismName);
            Assert.False(client.HasInitialResponse);
        }

        [Fact]
        public void Unknown_ReturnsNull()
        {
            var client = SaslClientFactory.CreateLdapClient("ngiurehigbrehier", "unused", "unused", new byte[] { 0x00 }, new Hashtable());
            Assert.Null(client);
        }
    }
}
