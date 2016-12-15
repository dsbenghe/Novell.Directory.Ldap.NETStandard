using Microsoft.Extensions.Logging;

namespace Novell.Directory.Ldap
{
    public static class Logger
    {
        private static ILoggerFactory _loggerFactory = new LoggerFactory().AddDebug();

        public static ILoggerFactory Factory
        {
            get { return _loggerFactory; }
            set { _loggerFactory = value; }
        }
    }
}