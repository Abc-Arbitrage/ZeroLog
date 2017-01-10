using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    [Ignore("Manual")]
    public class PerformanceTests
    {
        [SetUp]
        public void SetUp()
        {
//            LogManager.Initialize(new[] { new DateAndSizeRollingFileAppender("PerfTest"), }, 100 * 1024);
            LogManager.Initialize(new[] { new NullAppender(), }, 100 * 1024);
            Thread.Sleep(1);
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_test_console()
        {
            LogManager.GetLogger(typeof(PerformanceTests)).Info().Append("Hello ").Append(42).Append(" this is a relatlively long message ").Append(12345.4332m).Log();
        }

        [Test]
        public void should_run_test()
        {
            const int count = 100000;
            const int threads = 4;

            var timer = Stopwatch.StartNew();

            var logger = LogManager.GetLogger(typeof(PerformanceTests));

            Parallel.For(0, threads, i =>
            {
                for (int k = 0; k < count; k++)
                {
                    Thread.Sleep(1);
                    var logEvent = logger.Info().Append("Hello ").Append(42).Append(" this is a relatlively long message ").Append(12345.4332m);
                    logEvent.Log();
                }
            });

            LogManager.Shutdown();
            timer.Stop();
            Console.WriteLine("Log  : {0} us/log", timer.ElapsedMilliseconds * 1000.0 / (count * threads));
        }

    }
}
