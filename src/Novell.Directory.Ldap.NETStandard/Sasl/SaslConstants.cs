namespace Novell.Directory.Ldap.Sasl
{
#pragma warning disable CA1034 // Nested types should not be visible
    public static class SaslConstants
    {
        public static class Mechanism
        {
            public const string CramMd5 = "CRAM-MD5";
            public const string DigestMd5 = "DIGEST-MD5";

            /// <summary>
            /// Kerberos
            /// </summary>
            public const string GssApi = "GSSAPI";
            public const string Plain = "PLAIN";
            public const string Ntlm = "NTLM";
            public const string GssSPNego = "GSS-SPNEGO";
        }
    }
}
