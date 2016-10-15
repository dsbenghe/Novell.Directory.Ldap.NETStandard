using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class VerifyPasswordTests
    {
        [Fact]
        public void Bind_ForExistingEntry_ShouldWork()
        {
            TestHelper.WithLdapConnection(
                ldapConnection =>
                {
                    ldapConnection.Bind(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                });
        }
    }
}