// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Novell.Directory.Ldap.Utilclass;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests
{
    public class ResourcesHandlerTests
    {
        [Fact]
        public void GetResultString_when_known_error_code_returns_message()
        {
            Assert.Equal(ResultCodeMessages.GetResultCode("1"), ResourcesHandler.GetResultString(1));
        }

        [Fact]
        public void GetResultString_when_unknown_error_code_returns_unknown_message()
        {
            Assert.Contains("unknown", ResourcesHandler.GetResultString(int.MaxValue).ToLower());
        }
    }
}
