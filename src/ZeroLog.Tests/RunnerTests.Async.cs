using System;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class AsyncRunnerTests
{
    private TestAppender _testAppender;
    private AsyncRunner _runner;
    private Log _log;

    [SetUp]
    public void SetUpFixture()
    {
        _testAppender = new TestAppender(true);

        var config = new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            LogMessageBufferSize = 256,
            AppendingStrategy = AppendingStrategy.Asynchronous,
            RootLogger =
            {
                Appenders = { _testAppender }
            }
        };

        _runner = new AsyncRunner(config);

        _log = new Log(nameof(AsyncRunnerTests));
        _log.UpdateConfiguration(_runner, config);
    }

    [TearDown]
    public void Teardown()
    {
        _runner.Dispose();
    }

    [Test]
    public void should_flush_appenders_when_not_logging_messages()
    {
        var signal = _testAppender.SetMessageCountTarget(3);
        _testAppender.WaitOnWriteEvent = new ManualResetEventSlim(false);

        _log.Info("Foo");
        _log.Info("Bar");
        _log.Info("Baz");

        _testAppender.WaitOnWriteEvent.Set();
        signal.Wait(TimeSpan.FromSeconds(1));

        Wait.Until(() => _testAppender.FlushCount == 1, TimeSpan.FromSeconds(1));

        _log.Info("Foo");
        Wait.Until(() => _testAppender.FlushCount == 2, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void should_apply_configuration_updates()
    {
        _runner.UpdateConfiguration(new ZeroLogConfiguration
        {
            NullDisplayString = "Foo"
        });

        _runner.WaitUntilNewConfigurationIsApplied();

        _log.Info(null);

        Wait.Until(() => _testAppender.LoggedMessages.Count == 1, TimeSpan.FromSeconds(1));
        _testAppender.LoggedMessages.ShouldHaveSingleItem().ShouldEqual("Foo");
    }
}
