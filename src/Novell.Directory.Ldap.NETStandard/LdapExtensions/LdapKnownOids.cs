using System.Collections.Generic;

namespace Novell.Directory.Ldap
{
    public static class LdapKnownOids
    {
        public static class Extensions
        {
            public const string WhoAmI = "1.3.6.1.4.1.4203.1.11.3";
            public const string StartTls = "1.3.6.1.4.1.1466.20037";
            public const string DynamicRefresh = "1.3.6.1.4.1.1466.101.119.1";
            public const string FastConcurrentBind = "1.2.840.113556.1.4.1781";
            public const string ModifyPassword = "1.3.6.1.4.1.4203.1.11.1";
            public const string GracefulDisconnect = "1.3.6.1.4.1.18060.0.1.5";
            public const string GracefulShutdownRequest = "1.3.6.1.4.1.18060.0.1.3";
            public const string NoticeOfDisconnection = "1.3.6.1.4.1.1466.20036";
        }

        private static IReadOnlyDictionary<string, string> _oidNames = new Dictionary<string, string>
        {
            [Extensions.WhoAmI] = "Who am I",
            [Extensions.StartTls] = "Start TLS",
            [Extensions.DynamicRefresh] = "Dynamic Refresh",
            [Extensions.FastConcurrentBind] = "Fast concurrent Bind",
            [Extensions.ModifyPassword] = "Modify Password",
            [Extensions.GracefulDisconnect] = "Graceful Disconnect",
            [Extensions.GracefulShutdownRequest] = "Graceful Shutdown Request",
            [Extensions.NoticeOfDisconnection] = "Notice of Disconnection",
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
