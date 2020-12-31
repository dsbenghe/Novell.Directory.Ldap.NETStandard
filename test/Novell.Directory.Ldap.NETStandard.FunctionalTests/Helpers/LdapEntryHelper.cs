using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers
{
    public static class LdapEntryHelper
    {
        public static LdapEntry NewLdapEntry(string cnPrefix = null)
        {
            var cn = Guid.NewGuid().ToString();
            if (cnPrefix != null)
            {
                cn = cnPrefix + "_" + cn;
            }

            var attributeSet = new LdapAttributeSet
            {
                new LdapAttribute("cn", cn),
                new LdapAttribute("objectClass", TestsConfig.DefaultObjectClass),
                new LdapAttribute("givenName", "Lionel"),
                new LdapAttribute("sn", "Messi"),
                new LdapAttribute("mail", cn + "@gmail.com"),
                new LdapAttribute("userPassword", TestsConfig.DefaultPassword),
            };

            var dn = TestHelper.BuildDn(cn);
            return new LdapEntry(dn, attributeSet);
        }

        public static void AssertSameAs(this LdapEntry expectedEntry, LdapEntry actualEntry)
        {
            Assert.Equal(expectedEntry.Dn, actualEntry.Dn);
            var expectedAttributes = expectedEntry.GetAttributeSet();
            var actualAttributes = actualEntry.GetAttributeSet();
            expectedAttributes.AssertSameAs(actualAttributes);
        }

        private static void AssertSameAs(this LdapAttributeSet expectedAttributeSet, LdapAttributeSet actualAttributeSet)
        {
            AssertSameAs(expectedAttributeSet, actualAttributeSet, new List<string>());
        }

        public static void AssertSameAs(this LdapAttributeSet expectedAttributeSet, LdapAttributeSet actualAttributeSet, List<string> excludeAttributes)
        {
            Assert.Equal(expectedAttributeSet.Count, actualAttributeSet.Count);
            foreach (LdapAttribute expectedAttribute in expectedAttributeSet)
            {
                if (excludeAttributes.Contains(expectedAttribute.Name))
                {
                    continue;
                }

                var actualAttribute = actualAttributeSet.GetAttribute(expectedAttribute.Name);
                actualAttribute.ByteValues.Should().BeEquivalentTo(expectedAttribute.ByteValues);
            }
        }
    }
}
