using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class FetchSchemaTests
    {
        [Fact]
        public void FetchSchema_returns_the_default_schema()
        {
            var schema = TestHelper.WithAuthenticatedLdapConnection(ldapConnection =>
            {
                var ldapConnectionImpl = (LdapConnection)ldapConnection;
                return ldapConnectionImpl.FetchSchema(ldapConnectionImpl.GetSchemaDn());
            });

            Assert.NotNull(schema);
            Assert.True(schema.AttributeNames.ToEnumerable().Any());
            Assert.True(schema.AttributeSchemas.ToEnumerable().Any());
            Assert.True(schema.ObjectClassNames.ToEnumerable().Any());
            Assert.True(schema.ObjectClassSchemas.ToEnumerable().Any());
        }
    }

    public static class EnumeratorExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
}
