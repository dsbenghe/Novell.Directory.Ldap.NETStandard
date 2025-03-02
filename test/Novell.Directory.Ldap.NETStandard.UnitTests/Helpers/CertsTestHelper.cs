// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using Xunit;

namespace Novell.Directory.Ldap.NETStandard.UnitTests.Helpers
{
    public static class CertsTestHelper
    {
        public static byte[] GetCertificate(string name)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var manifestResourceStream = executingAssembly.GetManifestResourceStream($"{executingAssembly.GetName().Name}.certs.{name}");

            if (manifestResourceStream == null)
            {
                throw new ArgumentNullException(nameof(manifestResourceStream));
            }

            var certBytes = new byte[manifestResourceStream.Length];
            var retBytes = manifestResourceStream.Read(certBytes, 0, certBytes.Length);
            Assert.Equal(retBytes, certBytes.Length);

            return certBytes;
        }
    }
}
