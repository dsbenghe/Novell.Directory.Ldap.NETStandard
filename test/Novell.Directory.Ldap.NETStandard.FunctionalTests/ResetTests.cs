// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Novell.Directory.Ldap.NETStandard.FunctionalTests.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.FunctionalTests
{
    public class ResetTests
    {
        [Fact]
        public async Task Reset_OnConnectedConnection_ReturnsTrue()
        {
            await TestHelper.WithLdapConnectionAsync(async ldapConnection =>
            {
                var result = ((LdapConnection)ldapConnection).TryReset();

                Assert.True(result);

                await Task.CompletedTask;
            });
        }

        [Fact]
        public async Task Reset_AfterAuthenticatedOperation_ConnectionRemainsUsable()
        {
            var ldapEntry = LdapEntryHelper.NewLdapEntry();

            await TestHelper.WithAuthenticatedLdapConnectionAsync(async ldapConnection =>
            {
                await ldapConnection.AddAsync(ldapEntry);

                var result = ((LdapConnection)ldapConnection).TryReset();
                Assert.True(result);

                // Simulates pool reuse: the same connection object is used
                // for a further operation immediately after Reset().
                var readEntry = await ldapConnection.ReadAsync(ldapEntry.Dn);

                Assert.Equal(ldapEntry.Dn, readEntry.Dn);

                // objectClass and userPassword are excluded: OpenDJ expands objectClass
                // to include superclasses (top, organizationalPerson, person) and hashes
                // userPassword on write, so neither round-trips byte-for-byte.
                ldapEntry.GetAttributeSet().AssertSameAs(
                    readEntry.GetAttributeSet(),
                    new List<string> { "objectClass", "userPassword" });
            });
        }
    }
}
