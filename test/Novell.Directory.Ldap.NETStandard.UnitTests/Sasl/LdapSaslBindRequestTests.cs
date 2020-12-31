using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Sasl
{
    public class LdapSaslBindRequestTests
    {
        [Fact]
        public void LdapSaslBindRequest_can_create_string_for_debugging()
        {
            var mechanism = Guid.NewGuid().ToString();
            var ldapSaslBindRequest = new LdapSaslBindRequest(3, mechanism, null);

            var str = ldapSaslBindRequest.ToString();

            Assert.Contains(mechanism, str);
        }
    }
}
