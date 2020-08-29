using System;
using Microsoft.Extensions.Logging;

namespace Novell.Directory.Ldap
{
    public static class Logger
    {
        private static ILoggerFactory _loggerFactory;

        static Logger()
        {
            Log = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
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