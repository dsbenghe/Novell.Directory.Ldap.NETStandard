// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests;

public class LdapEntryTests
{
    private LdapEntry _ldapEntry = NewLdapEntry();

    [Fact]
    public void Contains_when_exists_returns_true()
    {
        Assert.True(_ldapEntry.Contains("givenName"));
    }

    [Fact]
    public void Contains_when_not_exists_returns_false()
    {
        Assert.False(_ldapEntry.Contains("givenName_1"));
    }

    [Fact]
    public void GetOrDefault_when_exists_returns_attribute()
    {
        Assert.Equal("Lionel", _ldapEntry.GetOrDefault("givenName").StringValue);
    }

    [Fact]
    public void GetOrDefault_when_not_exists_returns_fallback()
    {
        var fallback = new LdapAttribute("name", "value");
        Assert.Equal(fallback, _ldapEntry.GetOrDefault("givenName_1", fallback));
    }

    [Fact]
    public void GetOrDefault_when_not_exists_without_fallback_returns_null()
    {
        Assert.Null(_ldapEntry.GetOrDefault("givenName_1"));
    }

    [Fact]
    public void TryGet_when_exists_returns_true_and_attribute()
    {
        Assert.True(_ldapEntry.TryGet("givenName", out var attribute));
        Assert.Equal("Lionel", attribute.StringValue);
    }

    [Fact]
    public void TryGet_when_not_exists_returns_false_and_null()
    {
        Assert.False(_ldapEntry.TryGet("givenName_1", out var attribute));
        Assert.Null(attribute);
    }

    [Fact]
    public void TryGet_is_case_sensitive_like_Get()
    {
        // Dictionary-backed lookups use the exact key; document that TryGet does not relax this.
        Assert.Equal(_ldapEntry.Contains("GIVENNAME"), _ldapEntry.TryGet("GIVENNAME", out _));
    }

    [Fact]
    public void AttributeSet_TryGetAttribute_matches_entry_TryGet()
    {
        var set = _ldapEntry.GetAttributeSet();
        Assert.True(set.TryGetAttribute("givenName", out var fromSet));
        Assert.True(_ldapEntry.TryGet("givenName", out var fromEntry));
        Assert.Same(fromSet, fromEntry);
        Assert.False(set.TryGetAttribute("givenName_1", out var missing));
        Assert.Null(missing);
    }

    [Fact]
    public void GetStringValueOrDefault_when_exists_returns_string_value()
    {
        Assert.Equal("Lionel", _ldapEntry.GetStringValueOrDefault("givenName"));
    }

    [Fact]
    public void GetStringValueOrDefault_when_not_exists_returns_fallback()
    {
        var fallback = "myvalue";
        Assert.Equal(fallback, _ldapEntry.GetStringValueOrDefault("givenName_1", fallback));
    }

    [Fact]
    public void GetStringValueOrDefault_when_not_exists_without_fallback_returns_null()
    {
        Assert.Null(_ldapEntry.GetStringValueOrDefault("givenName_1"));
    }

    [Fact]
    public void GetBytesOrDefault_when_exists_returns_string_value()
    {
        Assert.Equal(new byte[] { 1, 2 }, _ldapEntry.GetBytesValueOrDefault("bytes"));
    }

    [Fact]
    public void GetBytesOrDefault_when_not_exists_returns_fallback()
    {
        var fallback = new byte[] { 1, 2, 3 };
        Assert.Equal(fallback, _ldapEntry.GetBytesValueOrDefault("bytes_1", fallback));
    }

    [Fact]
    public void GetBytesOrDefault_when_not_exists_without_fallback_returns_null()
    {
        Assert.Null(_ldapEntry.GetBytesValueOrDefault("bytes_1"));
    }

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
            new LdapAttribute("givenName", "Lionel"),
            new LdapAttribute("sn", "Messi"),
            new LdapAttribute("mail", cn + "@gmail.com"),
            new LdapAttribute("bytes", new byte[] { 1, 2 }),
        };

        var dn = $"cn={cn}";
        return new LdapEntry(dn, attributeSet);
    }
}
