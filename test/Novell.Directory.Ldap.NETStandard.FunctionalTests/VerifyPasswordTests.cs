using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class VerifyPasswordTests
    {
        [Fact]
        public async Task Bind_ForExistingEntry_ShouldWork()
        {
            await TestHelper.WithLdapConnectionAsync(
                async ldapConnection =>
                {
                    await ldapConnection.BindAsync(TestsConfig.LdapServer.RootUserDn, TestsConfig.LdapServer.RootUserPassword);
                });
        }

        [Fact]
        public async Task Bind_WithWrongPassword_ShouldThrowInvalidCredentials()
        {
            var ldapException = await Assert.ThrowsAsync<LdapException>(async () =>
                await TestHelper.WithLdapConnectionAsync(
                    async ldapConnection =>
                    {
                        await ldapConnection.BindAsync(
                            TestsConfig.LdapServer.RootUserDn,
                            TestsConfig.LdapServer.RootUserPassword + "1");
                    }));
            Assert.Equal(LdapException.InvalidCredentials, ldapException.ResultCode);
        }
    }
}
