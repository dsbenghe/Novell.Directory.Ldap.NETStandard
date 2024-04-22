using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class FetchSchemaTests
    {
        [Fact]
        public async Task FetchSchema_returns_the_default_schema()
        {
            var schema = await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                var ldapConnectionImpl = (LdapConnection)ldapConnection;
                return await ldapConnectionImpl.FetchSchemaAsync(await ldapConnectionImpl.GetSchemaDnAsync());
            });

            Assert.NotNull(schema);
            Assert.NotEmpty(schema.AttributeNames);
            Assert.NotEmpty(schema.AttributeSchemas);
            Assert.NotEmpty(schema.ObjectClassNames);
            Assert.NotEmpty(schema.ObjectClassSchemas);
        }
    }
}
