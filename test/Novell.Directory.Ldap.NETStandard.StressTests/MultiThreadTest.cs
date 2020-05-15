using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Novell.Directory.Ldap.NETStandard.StressTests
{
    public class MultiThreadTest
    {
        private const double PercentOfAcceptedLdapExceptions = 0.02;
        private static readonly TimeSpan DefaultTestingThreadReportingPeriod = TimeSpan.FromMinutes(1);

        private readonly int _noOfThreads;
        private readonly TimeSpan _timeToRun;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MultiThreadTest> _logger;
        private readonly TimeSpan _monitoringThreadReportingPeriod = TimeSpan.FromSeconds(300);

        private static readonly List<ExceptionInfo> Exceptions = new List<ExceptionInfo>();

        public MultiThreadTest(int noOfThreads, TimeSpan timeToRun, ILoggerFactory loggerFactory)
        {
            _noOfThreads = noOfThreads;
            _timeToRun = timeToRun;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MultiThreadTest>();
        }

        public int Run()
        {
            var threads = new Thread[_noOfThreads];
            var threadDatas = new ThreadRunner[_noOfThreads];
            for (var i = 0; i < _noOfThreads; i++)
            {
                var threadRunner = new ThreadRunner(DefaultTestingThreadReportingPeriod, _loggerFactory.CreateLogger<ThreadRunner>());
                threads[i] = new Thread(threadRunner.RunLoop);
                threadDatas[i] = threadRunner;
                threads[i].Start();
            }

            var monitoringThread = new Thread(MonitoringThread);
            var monitoringThreadData = new MonitoringThreadData(threadDatas);
            monitoringThread.Start(monitoringThreadData);

            Thread.Sleep(_timeToRun);
            _logger.LogInformation("Exiting worker threads");
            foreach (var threadData in threadDatas)
            {
                threadData.ShouldStop = true;
            }
            Thread.Sleep(TimeSpan.FromSeconds(60));
            foreach (var thread in threads)
            {
                thread.Join(TimeSpan.FromSeconds(1));
            }

            foreach (var thread in threads)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            _logger.LogInformation("Exiting monitoring thread");
            monitoringThreadData.WaitHandle.Set();
            monitoringThread.Join();

            var failRun = ReportRunResult(threadDatas);

            return failRun ? 1 : 0;
        }

        private bool ReportRunResult(ThreadRunner[] threadDatas)
        {
            var noOfRuns = threadDatas.Sum(x => x.Count);
            var noOfLdapExceptions = Exceptions.Count(x => (x.Ex as LdapException) != null);
            var noOfNonLdapExceptions = Exceptions.Count - noOfLdapExceptions;
            var percentOfLdapExceptions = (float) noOfLdapExceptions * 100 / noOfRuns;
            var failRun = noOfNonLdapExceptions > 0 || percentOfLdapExceptions > PercentOfAcceptedLdapExceptions;
            _logger.LogInformation(
                $"Number of test runs = {noOfRuns} on {_noOfThreads} threads, no of exceptions: {Exceptions.Count}, no of non ldap exceptions {noOfNonLdapExceptions}, fail {failRun}");
            return failRun;
        }

        private void MonitoringThread(object param)
        {
            var monitoringThreadData = (MonitoringThreadData)param;
            do
            {
                DumpStats(monitoringThreadData);
            } while (!monitoringThreadData.WaitHandle.WaitOne(_monitoringThreadReportingPeriod));
            DumpStats(monitoringThreadData);
        }

        private void DumpStats(MonitoringThreadData monitoringThreadData)
        {
            var logMessage = new StringBuilder();
            logMessage.Append("Monitoring thread [threadId:noOfRuns:lastUpdateSecondsAgo:possibleHanging]:");
            foreach (var threadRunner in monitoringThreadData.ThreadRunners)
            {
                int threadId;
                int count;
                DateTime lastDate;
                lock (threadRunner)
                {
                    threadId = threadRunner.ThreadId;
                    count = threadRunner.Count;
                    lastDate = threadRunner.LastPingDate;
                }

                var lastUpdateSecondsAgo = (int)(DateTime.Now - lastDate).TotalSeconds;
                var possibleHanging = (lastUpdateSecondsAgo - 2 * DefaultTestingThreadReportingPeriod.TotalSeconds) > 0;
                logMessage.AppendFormat("[{0}-{1}-{2}-{3}]", threadId, count, lastUpdateSecondsAgo, possibleHanging ? "!!!!!!" : "_");
            }

            _logger.LogInformation(logMessage.ToString());
        }

        private class ThreadRunner
        {
            public int ThreadId;

            public ThreadRunner(TimeSpan testingThreadReportingPeriod, ILogger<ThreadRunner> logger)
            {
                _testingThreadReportingPeriod = testingThreadReportingPeriod;
                _logger = logger;                
                Count = 0;
                ShouldStop = false;
                LastPingDate = DateTime.Now;
            }

            public DateTime LastPingDate;
            public int Count;
            public bool ShouldStop;
            private readonly TimeSpan _testingThreadReportingPeriod;
            private readonly ILogger<ThreadRunner> _logger;

            public void RunLoop()
            {
                ThreadId = Thread.CurrentThread.ManagedThreadId;
                var rnd = new Random();
                var i = 0;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                while (!ShouldStop)
                {
                    try
                    {
                        var test = TestsToRun.Tests[rnd.Next() % TestsToRun.Tests.Count];
                        test();
                    }
                    catch (Exception ex)
                    {
                        ReportException(ex);
                    }

                    i++;
                    ReportAliveness(stopWatch, i);
                }
            }

            private void ReportAliveness(Stopwatch stopWatch, int i)
            {
                if (stopWatch.Elapsed > _testingThreadReportingPeriod)
                {
                    stopWatch.Stop();
                    lock (this)
                    {
                        Count = i;
                        LastPingDate = DateTime.Now;
                    }

                    stopWatch.Restart();
                }
            }

            private void ReportException(Exception ex)
            {
                _logger.LogError("Error in runner thread - {0}", ex);
                if (ex is TargetInvocationException && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                lock (Exceptions)
                {
                    Exceptions.Add(new ExceptionInfo
                    {
                        Ex = ex,
                        ThreadId = Thread.CurrentThread.ManagedThreadId
                    });
                }
            }
        }

        private class MonitoringThreadData
        {
            public MonitoringThreadData(ThreadRunner[] threadRunners)
            {
                ThreadRunners = threadRunners;
                WaitHandle = new AutoResetEvent(false);
            }

            public readonly EventWaitHandle WaitHandle;

            public ThreadRunner[] ThreadRunners { get; }
        }

        public class ExceptionInfo
        {
            public Exception Ex { get; set; }

            public long ThreadId { get; set; }
        }
    }
}