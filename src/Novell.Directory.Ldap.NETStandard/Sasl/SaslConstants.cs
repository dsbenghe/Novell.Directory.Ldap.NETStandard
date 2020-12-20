namespace Novell.Directory.Ldap.Sasl
{
#pragma warning disable CA1034 // Nested types should not be visible
    public static class SaslConstants
    {
        /// <summary>
        /// IANA Has a list of "officially" reserved mechanisms:
        /// https://www.iana.org/assignments/sasl-mechanisms/sasl-mechanisms.xhtml
        ///
        /// Mechanisms in this list are supported by <see cref="DefaultSaslClientFactory"/>,
        /// although at the time of writing not all are implemented yet.
        ///
        /// If you want to add support for additional mechanisms (or replace our default
        /// implementation), see <see cref="ISaslClientFactory"/> and
        /// <see cref="ILdapConnection.RegisterSaslClientFactory(ISaslClientFactory)"/>.
        /// </summary>
        public static class Mechanism
        {
            /// <summary>
            /// RFC 4505:
            /// https://tools.ietf.org/html/rfc4505
            ///
            /// Obsoletes RFC 2245:
            /// https://tools.ietf.org/html/rfc2245.
            /// </summary>
            public const string Anonymous = "ANONYMOUS";

            /// <summary>
            /// RFC 2195:
            /// https://tools.ietf.org/html/rfc2195
            ///
            /// Obsoletes RFC 2095:
            /// https://tools.ietf.org/html/rfc2195.
            /// </summary>
            public const string CramMd5 = "CRAM-MD5";

            /// <summary>
            /// RFC 2831:
            /// https://tools.ietf.org/html/rfc2831.
            /// </summary>
            public const string DigestMd5 = "DIGEST-MD5";

            /// <summary>
            /// RFC 4422:
            /// https://tools.ietf.org/html/rfc4422
            ///
            /// Obsoletes RFC 2222:
            /// https://tools.ietf.org/html/rfc2222.
            /// </summary>
            public const string External = "EXTERNAL";

            /// <summary>
            /// Kerberos V5
            ///
            /// RFC 4752:
            /// https://tools.ietf.org/html/rfc4752
            ///
            /// Theoretically could be used for other mechanisms,
            /// but only Kerberos V5 or <see cref="GssSPNego"/> is
            /// in wide use.
            ///
            /// There is a "GS2" family of mechanisms that tries to
            /// remedy this, specified in RFC 5801:
            /// https://tools.ietf.org/html/rfc5801
            ///
            /// For the purposes of this LDAP Client however, GSSAPI
            /// means Kerberos V5, and the Client may not even use
            /// a GSS-API implementation to do so.
            /// </summary>
            public const string GssApi = "GSSAPI";

            /// <summary>
            /// RFC 4178:
            /// https://tools.ietf.org/html/rfc4178
            ///
            /// Obsoletes RFC 2478:
            /// https://tools.ietf.org/html/rfc2478.
            /// </summary>
            public const string GssSPNego = "GSS-SPNEGO";

            /// <summary>
            /// The PLAIN SASL mechanism does not provide a security layer.
            ///
            /// The PLAIN mechanism should not be used without adequate data security
            /// protection as this mechanism affords no integrity or confidentiality
            /// protections itself.  The mechanism is intended to be used with data
            /// security protections provided by application-layer protocol,
            /// generally through its use of Transport Layer Security (TLS)
            /// services.
            ///
            /// RFC 4616:
            /// https://tools.ietf.org/html/rfc4616.
            /// </summary>
            public const string Plain = "PLAIN";
        }
    }
}
