using System;

namespace Novell.Directory.Ldap.NETStandard.StressTests
{
    public class Program
    {
        private static readonly int DefaultNoOfThreads = 10;
        private static readonly TimeSpan DefaultTimeToRun = TimeSpan.FromMinutes(10);

        public static int Main(string[] args)
        {
            //var loggerFactory = new LoggerFactory().AddConsole();

            var testsToBeRun = TestsToRun.GetMethods();
            Console.WriteLine("----Run stress test using the following tests");
            foreach (var test in testsToBeRun)
            {
                Console.WriteLine(test.Name);
            }

            var noOfThreads = DefaultNoOfThreads;
            var timeToRun = DefaultTimeToRun;
            foreach( var arg in args)
                Console.WriteLine(arg);
            if (args.Length >= 1)
                noOfThreads = int.Parse(args[0]);
            if(args.Length >= 2)
                timeToRun = TimeSpan.FromMinutes(int.Parse(args[1]));

            Console.WriteLine("----Running stress test with {0} threads for {1} minutes", noOfThreads, (int)timeToRun.TotalMinutes);
            var noOfExceptions = new MultiThreadTest(noOfThreads, timeToRun).Run();
            return noOfExceptions;
        }
    }
}