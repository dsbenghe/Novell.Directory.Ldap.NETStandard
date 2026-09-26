// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class LdapConnectionTests
    {
        [Fact]
        public void TryReset_on_unconnected_connection_returns_true()
        {
            using var ldapConnection = new LdapConnection();

            var result = ldapConnection.TryReset();

            Assert.True(result);
        }

        [Fact]
        public void TryReset_can_be_called_multiple_times_and_returns_true_each_time()
        {
            using var ldapConnection = new LdapConnection();

            var firstResult = ldapConnection.TryReset();
            var secondResult = ldapConnection.TryReset();

            Assert.True(firstResult);
            Assert.True(secondResult);
        }
    }
}
