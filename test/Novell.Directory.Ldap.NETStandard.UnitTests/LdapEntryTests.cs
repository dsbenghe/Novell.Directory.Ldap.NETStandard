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
