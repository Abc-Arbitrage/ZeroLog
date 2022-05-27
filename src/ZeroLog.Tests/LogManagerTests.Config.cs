using System;
using System.Linq;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

public partial class LogManagerTests
{
    [Test]
    public void should_apply_appender_changes()
    {
        var fooLog = LogManager.GetLogger("Foo");
        var barLog = LogManager.GetLogger("Bar");

        var barAppender = new TestAppender(true);

        _config.Loggers.Add(new LoggerConfiguration(fooLog.Name) { IncludeParentAppenders = false });
        _config.Loggers.Add(new LoggerConfiguration(barLog.Name) { Appenders = { barAppender } });

        ApplyConfigChanges();

        var rootSignal = _testAppender.SetMessageCountTarget(1);
        var barSignal = barAppender.SetMessageCountTarget(1);

        fooLog.Info("Foo");
        barLog.Info("Bar");

        rootSignal.Wait(TimeSpan.FromSeconds(1));
        barSignal.Wait(TimeSpan.FromSeconds(1));

        _testAppender.LoggedMessages.ShouldHaveSingleItem().ShouldEqual("Bar");
        barAppender.LoggedMessages.ShouldHaveSingleItem().ShouldEqual("Bar");
    }

    [Test]
    public void should_apply_log_level_changes()
    {
        var fooLog = LogManager.GetLogger("Foo");
        var barLog = LogManager.GetLogger("Bar");

        _config.Loggers.Add(new LoggerConfiguration(fooLog.Name) { Level = LogLevel.Warn });
        ApplyConfigChanges();

        fooLog.IsInfoEnabled.ShouldBeFalse();
        fooLog.IsWarnEnabled.ShouldBeTrue();

        barLog.IsInfoEnabled.ShouldBeTrue();
        barLog.IsWarnEnabled.ShouldBeTrue();
    }

    [Test]
    public void should_not_apply_changes_until_requested()
    {
        var log = LogManager.GetLogger<LogManagerTests>();

        _config.Loggers.Add(new LoggerConfiguration(log.Name) { Level = LogLevel.Warn });

        log.IsInfoEnabled.ShouldBeTrue();

        ApplyConfigChanges();

        log.IsInfoEnabled.ShouldBeFalse();
    }

    [Test]
    public void should_dispose_active_appenders_on_shutdown()
    {
        var loggerAppender = new TestAppender(false);

        _config.Loggers.Add(new LoggerConfiguration("Foo") { Appenders = { loggerAppender } });
        ApplyConfigChanges();

        LogManager.Shutdown();

        _testAppender.IsDisposed.ShouldBeTrue();
        loggerAppender.IsDisposed.ShouldBeTrue();
    }

    [Test]
    public void should_dispose_removed_appenders_on_shutdown()
    {
        var loggerAppender = new TestAppender(false);

        _config.Loggers.Add(new LoggerConfiguration("Foo") { Appenders = { loggerAppender } });
        ApplyConfigChanges();

        loggerAppender.IsDisposed.ShouldBeFalse();

        _config.Loggers.Single().Appenders.Clear();
        ApplyConfigChanges();

        loggerAppender.IsDisposed.ShouldBeFalse();

        _config.Loggers.Clear();
        ApplyConfigChanges();

        loggerAppender.IsDisposed.ShouldBeFalse();

        LogManager.Shutdown();

        loggerAppender.IsDisposed.ShouldBeTrue();
    }

    private void ApplyConfigChanges()
    {
        _config.ApplyChanges();
        _logManager.WaitUntilNewConfigurationIsApplied();
    }
}
