using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class AsyncRunnerTests
{
    private TestAppender _testAppender;
    private ZeroLogConfiguration _config;
    private AsyncRunner _runner;
    private Log _log;

    [SetUp]
    public void SetUpFixture()
    {
        _testAppender = new TestAppender(true);

        _config = new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            LogMessageBufferSize = 256,
            AppendingStrategy = AppendingStrategy.Asynchronous,
            RootLogger =
            {
                Appenders = { _testAppender }
            }
        };

        _runner = new AsyncRunner(_config);

        _log = new Log(nameof(AsyncRunnerTests));
        _log.UpdateConfiguration(_runner, _config);
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
    public void should_flush_appenders_explicitly()
    {
        _testAppender.WaitOnWriteEvent = new ManualResetEventSlim(false);

        _log.Info("Foo");
        _log.Info("Bar");
        _log.Info("Baz");

        var flushTask = Task.Factory.StartNew(() => _runner.Flush(), TaskCreationOptions.LongRunning);
        flushTask.Wait(TimeSpan.FromSeconds(1)).ShouldBeFalse();

        _testAppender.WaitOnWriteEvent.Set();

        flushTask.Wait(TimeSpan.FromSeconds(1)).ShouldBeTrue();
        _testAppender.FlushCount.ShouldEqual(1);
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

    [Test]
    public void should_not_log_pool_exhaustion_message_twice_in_a_row()
    {
        var firstMessage = _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders);
        firstMessage.ConstantMessage.ShouldBeNull();

        for (var i = 1; i < _config.LogMessagePoolSize; ++i)
            _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ConstantMessage.ShouldBeNull();

        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ConstantMessage!.ShouldContain("Log message pool is exhausted");
        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ShouldBeTheSameAs(LogMessage.Empty);
        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ShouldBeTheSameAs(LogMessage.Empty);

        _runner.Submit(firstMessage);
        _runner.Flush();
        Thread.Sleep(500);

        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ConstantMessage.ShouldBeNull();
        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ConstantMessage!.ShouldContain("Log message pool is exhausted");
        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ShouldBeTheSameAs(LogMessage.Empty);
        _runner.AcquireLogMessage(LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders).ShouldBeTheSameAs(LogMessage.Empty);
    }
}
