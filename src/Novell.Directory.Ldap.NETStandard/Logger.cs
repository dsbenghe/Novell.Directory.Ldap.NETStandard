using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Novell.Directory.Ldap
{
    public static class Logger
    {
        private static ILoggerFactory _loggerFactory;

        static Logger()
        {
            Log = NullLogger.Instance;
        }

        public static ILoggerFactory Factory
        {
            get => _loggerFactory;
            set
            {
                _loggerFactory = value;
                Init();
            }
        }

        public static ILogger Log { get; private set; }

        public static void LogWarning(this ILogger logger, string message, Exception ex)
        {
            logger.LogWarning(message + " - {0}", ex.ToString());
        }

        private static void Init()
        {
            Log = _loggerFactory.CreateLogger("Ldap");
        }
    }
}
