using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    [Ignore("Manual")]
    public class IntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            LogManager.Initialize(new[] { new ConsoleAppender() }, 100 * 1024);
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
            LogManager.GetLogger(typeof(IntegrationTests)).Info().Append("Hello").Log();
        }

        [Test]
        public void should_not_allocate()
        {
            const int count = 1000000;

            GC.Collect(2, GCCollectionMode.Forced, true);
            var gcCount = GC.CollectionCount(0);
//            Console.ReadKey();

            var timer = Stopwatch.StartNew();

            var logger = LogManager.GetLogger(typeof(IntegrationTests));
            for (int k = 0; k < count; k++)
            {
                Thread.Sleep(0);
                logger.Info().Append("Hello").Log();
            }
            LogManager.Shutdown();
            timer.Stop();
            Console.WriteLine("BCL  : {0} us/log", timer.ElapsedMilliseconds * 1000.0 / count);
            Console.WriteLine("GCs  : {0}", GC.CollectionCount(0) - gcCount);
        }
    }
}
