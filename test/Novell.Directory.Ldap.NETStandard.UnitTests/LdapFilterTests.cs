// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Novell.Directory.Ldap.Rfc2251;
using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests;

public class LdapFilterTests
{
    [Theory]
    [InlineData("*", @"\2a")]
    [InlineData("(", @"\28")]
    [InlineData(")", @"\29")]
    [InlineData(@"\", @"\5c")]
    [InlineData("\0", @"\00")]
    [InlineData("\n", @"\0a")]
    public void EscapeValue_escapes_rfc4515_special_characters(string input, string expected)
    {
        Assert.Equal(expected, LdapFilter.EscapeValue(input));
    }

    [Theory]
    [InlineData("jdoe")]
    [InlineData("John Doe")]
    [InlineData("Ünal Şen")]
    [InlineData("a=b,c+d")]
    public void EscapeValue_leaves_ordinary_and_non_ascii_text_untouched(string input)
    {
        Assert.Same(input, LdapFilter.EscapeValue(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EscapeValue_treats_null_and_empty_as_empty(string input)
    {
        Assert.Equal(string.Empty, LdapFilter.EscapeValue(input));
    }

    [Fact]
    public void EscapeValue_bytes_encodes_every_byte()
    {
        Assert.Equal(@"\01\ab\ff", LdapFilter.EscapeValue(new byte[] { 0x01, 0xAB, 0xFF }));
        Assert.Equal(string.Empty, LdapFilter.EscapeValue((byte[])null));
    }

    [Fact]
    public void Format_escapes_arguments_but_not_template()
    {
        var filter = LdapFilter.Format("(&(objectClass=person)(uid={0}))", "*)(uid=*");
        Assert.Equal(@"(&(objectClass=person)(uid=\2a\29\28uid=\2a))", filter);
    }

    [Fact]
    public void Format_handles_null_bytes_and_other_types()
    {
        Assert.Equal("(a=)(b=\\01)(c=42)", LdapFilter.Format("(a={0})(b={1})(c={2})", null, new byte[] { 1 }, 42));
    }

    [Fact]
    public void Format_requires_template()
    {
        Assert.Throws<ArgumentNullException>(() => LdapFilter.Format(null, "x"));
    }

    [Fact]
    public void Injection_attempt_parses_as_a_single_equality_match()
    {
        // Without escaping this input would turn one assertion into two.
        var filter = new RfcFilter(LdapFilter.Format("(uid={0})", "*)(objectClass=*"));
        Assert.Equal("(uid=*)(objectClass=*)", filter.FilterToString());
    }

    [Fact]
    public void Dn_read_from_directory_can_be_embedded_in_a_filter()
    {
        // Regression for #194 / #175: a DN-escaped value ("\,") must be filter-escaped again ("\5c,").
        const string memberDn = @"CN=Ryan\, Gaudion,CN=Users,DC=example,DC=com";
        var filter = LdapFilter.Format("(&(objectClass=group)(member={0}))", memberDn);

        Assert.Equal(@"(&(objectClass=group)(member=CN=Ryan\5c, Gaudion,CN=Users,DC=example,DC=com))", filter);
        var parsed = new RfcFilter(filter); // throws LdapLocalException "Filter Error" on the unescaped form
        Assert.Contains(memberDn, parsed.FilterToString());
    }
}
