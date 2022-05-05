using System;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture, NonParallelizable]
public class UninitializedLogManagerTests
{
    private TestAppender _testAppender;

    [SetUp]
    public void SetUpFixture()
    {
        _testAppender = new TestAppender(true);
    }

    [TearDown]
    public void Teardown()
    {
        LogManager.Shutdown();
    }

    [Test]
    public void should_log_without_initialize()
    {
        LogManager.GetLogger("Test").Info("Test");
    }

    [Test]
    public void should_log_correctly_when_logger_is_retrieved_before_log_manager_is_initialized()
    {
        var log = LogManager.GetLogger<LogManagerTests>();

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            RootLogger =
            {
                Appenders = { _testAppender }
            }
        });

        var signal = _testAppender.SetMessageCountTarget(1);

        log.Info("Lol");

        signal.Wait(TimeSpan.FromSeconds(1)).ShouldBeTrue();
    }
}
