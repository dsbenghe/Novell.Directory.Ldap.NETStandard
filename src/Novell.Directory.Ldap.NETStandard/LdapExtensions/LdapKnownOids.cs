using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    public static class LdapKnownOids
    {
        public static class Extensions
        {
            /// <summary>
            /// RFC 4532: LDAP "Who am I?" Operation
            /// https://tools.ietf.org/html/rfc4532.
            /// </summary>
            public const string WhoAmI = "1.3.6.1.4.1.4203.1.11.3";

            /// <summary>
            /// RFC 3063: LDAP Password Modify Extended Operation
            /// https://tools.ietf.org/html/rfc3062.
            /// </summary>
            public const string PasswordModify = "1.3.6.1.4.1.4203.1.11.1";

            /// <summary>
            /// LDAP_SERVER_BATCH_REQUEST_OID
            /// https://msdn.microsoft.com/en-us/library/jj217379.aspx.
            /// </summary>
            public const string ServerBatchRequest = "1.2.840.113556.1.4.2212";

            public const string StartTls = "1.3.6.1.4.1.1466.20037";
            public const string DynamicRefresh = "1.3.6.1.4.1.1466.101.119.1";
            public const string FastConcurrentBind = "1.2.840.113556.1.4.1781";
            public const string GracefulDisconnect = "1.3.6.1.4.1.18060.0.1.5";
            public const string GracefulShutdownRequest = "1.3.6.1.4.1.18060.0.1.3";
            public const string NoticeOfDisconnection = "1.3.6.1.4.1.1466.20036";
        }

        private static readonly IReadOnlyDictionary<string, string> _oidNames = new Dictionary<string, string>
        {
            [Extensions.WhoAmI] = "Who am I? (RFC 4532)",
            [Extensions.StartTls] = "Start TLS",
            [Extensions.DynamicRefresh] = "Dynamic Refresh",
            [Extensions.FastConcurrentBind] = "Fast concurrent Bind",
            [Extensions.PasswordModify] = "Password Modify (RFC 3062)",
            [Extensions.GracefulDisconnect] = "Graceful Disconnect",
            [Extensions.GracefulShutdownRequest] = "Graceful Shutdown Request",
            [Extensions.NoticeOfDisconnection] = "Notice of Disconnection",
            [Extensions.ServerBatchRequest] = "LDAP_SERVER_BATCH_REQUEST_OID",
        };

        public static string GetDisplayNameForOid(string oid)
        {
            if (_oidNames.TryGetValue(oid, out string displayName))
            {
                return displayName;
            }

            return oid;
        }
    }
}
