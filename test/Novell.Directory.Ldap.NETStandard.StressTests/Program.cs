using System;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Novell.Directory.Ldap.NETStandard.StressTests
{
    public class Program
    {
        private static readonly int DefaultNoOfThreads = 10;
        private static readonly TimeSpan DefaultTimeToRun = TimeSpan.FromMinutes(10);

        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithThreadId()
                .WriteTo.LiterateConsole(LogEventLevel.Verbose, "{Timestamp:HH:mm} [{Level}] ({ThreadId}) {Message}{NewLine}{Exception}")
                .CreateLogger();
            var loggerFactory = new LoggerFactory().AddSerilog();
            Logger.Factory = loggerFactory;

            var testsToBeRun = TestsToRun.GetMethods();
            Log.Logger.Information("----Run stress test using the following tests");
            foreach (var test in testsToBeRun)
            {
                Log.Logger.Information(test.Name);
            }

            var noOfThreads = DefaultNoOfThreads;
            var timeToRun = DefaultTimeToRun;
            if (args.Length >= 1)
            {
                noOfThreads = int.Parse(args[0]);
            }

            if (args.Length >= 2)
            {
                timeToRun = TimeSpan.FromMinutes(int.Parse(args[1]));
            }

            Log.Logger.Information("----Running stress test with {0} threads for {1} minutes", noOfThreads, (int)timeToRun.TotalMinutes);
            var noOfExceptions = new MultiThreadTest(noOfThreads, timeToRun, loggerFactory).Run();
            return noOfExceptions;
        }
    }
}