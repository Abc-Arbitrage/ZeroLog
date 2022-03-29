using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Configuration;

namespace ZeroLog.Tests;

[TestFixture]
[Ignore("Manual")]
public class PerformanceTests
{
    private TestAppender _testAppender;

    [SetUp]
    public void SetUp()
    {
        _testAppender = new TestAppender(false);

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 16384,
            LogMessageBufferSize = 512,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                Appenders = { _testAppender }
            }
        });
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
        const int threadMessageCount = 1000 * 1000;
        const int threadCount = 5;
        const int totalMessageCount = threadMessageCount * threadCount;

        var timer = Stopwatch.StartNew();

        var logger = LogManager.GetLogger(typeof(PerformanceTests));

        var signal = _testAppender.SetMessageCountTarget(totalMessageCount);
        var utcNow = DateTime.UtcNow;

        Parallel.For(0, threadCount, i =>
        {
            for (var k = 0; k < threadMessageCount; k++)
            {
                logger.Info().Append("Hello ").Append(42).Append(utcNow).Append(42.56).Append(" this is a relatlively long message ").Append(12345.4332m).Log();
            }
        });

        var timedOut = !signal.Wait(TimeSpan.FromSeconds(10));

        timer.Stop();
        if (timedOut)
            Assert.Fail("Timeout");

        Console.WriteLine($"Total message count  : {totalMessageCount:N0} messages");
        Console.WriteLine($"Thread message count : {threadMessageCount:N0} messages");
        Console.WriteLine($"Thread count         : {threadCount} threads");
        Console.WriteLine($"Elapsed time         : {timer.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"Message rate         : {totalMessageCount / timer.Elapsed.TotalSeconds:N0} m/s");
        Console.WriteLine($"Average log cost     : {timer.ElapsedMilliseconds * 1000 / (double)totalMessageCount:N3} µs");
    }
}
