using System;

namespace Novell.Directory.Ldap.Sasl
{
    [Flags]
    public enum QualityOfProtection
    {
        /// <summary>
        /// Sentinel Value
        /// </summary>
        Invalid = 0,

        // "auth"
        AuthenticationOnly = 1,

        // "auth-int"
        AuthenticationWithIntegrityProtection = 2,

        // "auth-conf"
        AuthenticationWithIntegrityAndPrivacyProtection = 4,
    }
}
