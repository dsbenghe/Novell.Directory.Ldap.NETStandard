using Microsoft.Extensions.Logging;

namespace Novell.Directory.Ldap
{
    public static class Logger
    {
        private static ILoggerFactory _loggerFactory;
        private static ILogger _log;

        static Logger()
        {
            Factory = new LoggerFactory().AddDebug();
        }

        public static ILoggerFactory Factory
        {
            get { return _loggerFactory; }
            set
            {
                _loggerFactory = value;
                Init();
            }
        }

        public static ILogger Log => _log;

        private static void Init()
        {
            _log = _loggerFactory.CreateLogger("Ldap");
        }
    }
}