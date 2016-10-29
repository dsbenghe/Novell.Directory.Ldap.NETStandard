using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Novell.Directory.Ldap.NETStandard.StressTests
{
    public class MultiThreadTest
    {
        private static readonly TimeSpan DefaultTestingThreadReportingPeriod = TimeSpan.FromMinutes(1);

        private readonly int _noOfThreads;
        private readonly TimeSpan _timeToRun;
        private readonly TimeSpan _monitoringThreadReportingPeriod = TimeSpan.FromSeconds(30);

        private static readonly List<ExceptionInfo> Exceptions = new List<ExceptionInfo>();

        public MultiThreadTest(int noOfThreads, TimeSpan timeToRun)
        {
            _noOfThreads = noOfThreads;
            _timeToRun = timeToRun;
        }

        public int Run()
        {
            var threads = new Thread[_noOfThreads];
            var threadDatas = new ThreadData[_noOfThreads];
            for (var i = 0; i < _noOfThreads; i++)
            {
                threads[i] = new Thread(RunLoop);
                var param = new ThreadData(threads[i].ManagedThreadId, DefaultTestingThreadReportingPeriod);
                threadDatas[i] = param;
                threads[i].Start(param);
            }
            var monitoringThread = new Thread(MonitoringThread);
            var monitoringThreadData = new MonitoringThreadData(threadDatas);
            monitoringThread.Start(monitoringThreadData);

            Thread.Sleep(_timeToRun);
            Console.WriteLine("Exiting worker threads");
            foreach (var threadData in threadDatas)
            {
                threadData.ShouldStop = true;
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            Console.WriteLine("Exiting monitoring thread");
            monitoringThreadData.WaitHandle.Set();
            monitoringThread.Join();

            var noOfRuns = threadDatas.Sum(x => x.Count);
            Console.WriteLine(string.Format("Number of test runs = {0} on {1} threads, no of exceptions: {2}", noOfRuns,
                _noOfThreads, Exceptions.Count));
            return Exceptions.Count;
        }

        private void MonitoringThread(object param)
        {
            var monitoringThreadData = (MonitoringThreadData) param;
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
            foreach (var threadData in monitoringThreadData.ThreadDatas)
            {
                int threadId;
                int count;
                DateTime lastDate;
                lock (threadData)
                {
                    threadId = threadData.ThreadId;
                    count = threadData.Count;
                    lastDate = threadData.LastPingDate;
                }
                var lastUpdateSecondsAgo = (int) (DateTime.Now - lastDate).TotalSeconds;
                var possibleHanging = (lastUpdateSecondsAgo - 2 * DefaultTestingThreadReportingPeriod.TotalSeconds) > 0;
                logMessage.AppendFormat("[{0}-{1}-{2}-{3}]", threadId, count, lastUpdateSecondsAgo, possibleHanging);
            }
            Console.WriteLine(logMessage);
        }

        private static void RunLoop(object param)
        {
            var threadData = (ThreadData) param;
            var rnd = new Random();
            var i = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (!threadData.ShouldStop)
            {
                try
                {
                    var test = TestsToRun.Tests[rnd.Next() % TestsToRun.Tests.Count];
                    test();
                }
                catch (Exception ex)
                {
                    lock (Exceptions)
                    {
                        Exceptions.Add(new ExceptionInfo
                        {
                            Ex = ex,
                            ThreadId = Thread.CurrentThread.ManagedThreadId
                        });
                    }
                    Console.WriteLine(Thread.CurrentThread.ManagedThreadId + ":" + ex);
                }
                i++;
                if (stopWatch.Elapsed > threadData.TestingThreadReportingPeriod)
                {
                    stopWatch.Stop();
                    lock (threadData)
                    {
                        threadData.Count = i;
                        threadData.LastPingDate = DateTime.Now;
                    }
                    Console.WriteLine("({0}-{1})", Thread.CurrentThread.ManagedThreadId, i);
                    stopWatch.Restart();
                }
            }
        }

        private class ThreadData
        {
            public readonly int ThreadId;

            public ThreadData(int threadId, TimeSpan testingThreadReportingPeriod)
            {
                ThreadId = threadId;
                TestingThreadReportingPeriod = testingThreadReportingPeriod;
                Count = 0;
                ShouldStop = false;
                LastPingDate = DateTime.Now;
            }

            public DateTime LastPingDate;
            public int Count;
            public bool ShouldStop;
            public readonly TimeSpan TestingThreadReportingPeriod;
        }

        private class MonitoringThreadData
        {
            public MonitoringThreadData(ThreadData[] threadDatas)
            {
                ThreadDatas = threadDatas;
                WaitHandle = new AutoResetEvent(false);
            }

            public readonly EventWaitHandle WaitHandle;

            public ThreadData[] ThreadDatas { get; }
        }

        public class ExceptionInfo
        {
            public Exception Ex { get; set; }
            public long ThreadId { get; set; }
        }
    }
}