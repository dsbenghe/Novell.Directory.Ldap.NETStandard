using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class LdapUrlTests
    {
        [Fact]
        public void Ctor_Null_Exception()
        {
            Assert.Throws<UriFormatException>(() => new LdapUrl(null));
        }

        [Fact]
        public void Ctor_EmptyString_Exception()
        {
            // This is technically a bug since the LdapUrl ctor should check and throw a proper exception
            Assert.Throws<IndexOutOfRangeException>(() => new LdapUrl(string.Empty));
        }

        [Fact]
        public void Ctor_StartsWithURL()
        {
            var url = new LdapUrl("url:ldap://foo.example.com/");
            Assert.Equal(389, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.False(url.Secure);
            Assert.Null(url.AttributeArray);
            Assert.Null(url.Extensions);
            Assert.Null(url.Filter);
            Assert.Equal(LdapConnection.ScopeBase, url.Scope);
        }

        [Fact]
        public void Ctor_Ldap()
        {
            var url = new LdapUrl("ldap://foo.example.com/");
            Assert.Equal(389, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.False(url.Secure);
            Assert.Null(url.AttributeArray);
            Assert.Null(url.Extensions);
            Assert.Null(url.Filter);
            Assert.Equal(LdapConnection.ScopeBase, url.Scope);
        }

        [Fact]
        public void Ctor_LdapS()
        {
            var url = new LdapUrl("ldaps://foo.example.com/");
            Assert.Equal(636, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.True(url.Secure);
            Assert.Null(url.AttributeArray);
            Assert.Null(url.Extensions);
            Assert.Null(url.Filter);
            Assert.Equal(LdapConnection.ScopeBase, url.Scope);
        }

        [Fact]
        public void Ctor_EnclosedWithBrackets()
        {
            var url = new LdapUrl("<ldap://foo.example.com/>");
            Assert.Equal(389, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.False(url.Secure);
            Assert.Null(url.AttributeArray);
            Assert.Null(url.Extensions);
            Assert.Null(url.Filter);
            Assert.Equal(LdapConnection.ScopeBase, url.Scope);
        }

        [Fact]
        public void Ctor_MissingCloseBracket_Exception()
        {
            Assert.Throws<UriFormatException>(() => new LdapUrl("<ldap://foo.example.com/"));
        }

        [Fact]
        public void Ctor_NotLdap_Exception()
        {
            Assert.Throws<UriFormatException>(() => new LdapUrl("http://foo.example.com/"));
        }

        [Fact]
        public void Ctor_WithDn()
        {
            var urlStr = "ldap://foo.example.com/cn=admin,ou=marketing,o=corporation";
            var url = new LdapUrl(urlStr);

            Assert.Equal(389, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.False(url.Secure);
            Assert.Null(url.AttributeArray);
            Assert.Null(url.Extensions);
            Assert.Null(url.Filter);
            Assert.Equal(LdapConnection.ScopeBase, url.Scope);
            Assert.Equal("cn=admin,ou=marketing,o=corporation", url.GetDn());
        }

        [Fact]
        public void Ctor_Complex()
        {
            var urlStr = "ldap://foo.example.com/cn=admin,ou=marketing,o=corporation?attr1,attr2,attr3?sub?(objectclass=*)?ext1,ext2,ext3";
            var url = new LdapUrl(urlStr);

            Assert.Equal(389, url.Port);
            Assert.Equal("foo.example.com", url.Host);
            Assert.False(url.Secure);
            Assert.Equal("cn=admin,ou=marketing,o=corporation", url.GetDn());
            Assert.Equal(LdapConnection.ScopeSub, url.Scope);
            Assert.Equal("(objectclass=*)", url.Filter);
            Assert.Equal(new[] { "attr1", "attr2", "attr3" }, url.AttributeArray);
            Assert.Equal(new[] { "ext1", "ext2", "ext3" }, url.Extensions);
        }

        [Fact]
        public void Ctor_ComplexTooManyFields_Exception()
        {
            var urlStr = "ldap://foo.example.com/cn=admin,ou=marketing,o=corporation?attr1,attr2,attr3?sub?(objectclass=*)?ext1,ext2,ext3?";

            // LdapUrl: URL has too many ? fields
            Assert.Throws<UriFormatException>(() => new LdapUrl(urlStr));
        }
    }
}
