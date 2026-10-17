using System;
using NUnit.Framework;
using Shouldly;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Configuration;

[TestFixture]
public class ResolvedLoggerConfigurationTests
{
    [Test]
    public void should_have_empty_config()
    {
        var emptyConfig = ResolvedLoggerConfiguration.Empty;
        emptyConfig.Level.ShouldBe(LogLevel.None);
        emptyConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.DropLogMessage);
        emptyConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Error).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Fatal).ShouldBeEmpty();

#if NET8_0_OR_GREATER
        emptyConfig.TimeProvider.ShouldBeSameAs(TimeProvider.System);
#endif
    }

    [Test]
    public void should_resolve_configuration()
    {
        var appenderA = new NoopAppender();
        var appenderB = new NoopAppender();
        var appenderC = new NoopAppender { Level = LogLevel.Warn };
        var appenderD = new NoopAppender();

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = LogLevel.Debug,
                Appenders = { appenderA }
            },
            Loggers =
            {
                new LoggerConfiguration("Foo")
                {
                    Level = LogLevel.Error,
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                    Appenders = { appenderB }
                },
                new LoggerConfiguration("Foo.Bar")
                {
                    Level = LogLevel.Info,
                    Appenders = { appenderB, appenderC }
                },
                new LoggerConfiguration("Foo.Bar.Clear")
                {
                    IncludeParentAppenders = false,
                    Appenders = { appenderD }
                },
                new LoggerConfiguration("Baz")
                {
                    Appenders =
                    {
                        new AppenderConfiguration(appenderC)
                        {
                            Level = LogLevel.Error
                        }
                    }
                }
            }
        };

        var rootConfig = config.ResolveLoggerConfiguration("SomethingElse");
        rootConfig.Level.ShouldBe(LogLevel.Debug);
        rootConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.Default);
        rootConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        rootConfig.GetAppenders(LogLevel.Debug).ShouldBe([appenderA], ignoreOrder: true);
        rootConfig.GetAppenders(LogLevel.Info).ShouldBe([appenderA], ignoreOrder: true);
        rootConfig.GetAppenders(LogLevel.Warn).ShouldBe([appenderA], ignoreOrder: true);
        rootConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA], ignoreOrder: true);
        rootConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA], ignoreOrder: true);

        var fooConfig = config.ResolveLoggerConfiguration("Foo");
        fooConfig.Level.ShouldBe(LogLevel.Error);
        fooConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA, appenderB], ignoreOrder: true);
        fooConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA, appenderB], ignoreOrder: true);

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldBe(LogLevel.Info);
        fooBarConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Info).ShouldBe([appenderA, appenderB], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Warn).ShouldBe([appenderA, appenderB, appenderC], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA, appenderB, appenderC], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA, appenderB, appenderC], ignoreOrder: true);

        var fooBarClearConfig = config.ResolveLoggerConfiguration("Foo.Bar.Clear");
        fooBarClearConfig.Level.ShouldBe(LogLevel.Info);
        fooBarClearConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarClearConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(LogLevel.Info).ShouldBe([appenderD], ignoreOrder: true);
        fooBarClearConfig.GetAppenders(LogLevel.Warn).ShouldBe([appenderD], ignoreOrder: true);
        fooBarClearConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderD], ignoreOrder: true);
        fooBarClearConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderD], ignoreOrder: true);

        var bazConfig = config.ResolveLoggerConfiguration("Baz");
        bazConfig.Level.ShouldBe(LogLevel.Debug);
        bazConfig.LogMessagePoolExhaustionStrategy.ShouldBe(LogMessagePoolExhaustionStrategy.Default);
        bazConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        bazConfig.GetAppenders(LogLevel.Debug).ShouldBe([appenderA], ignoreOrder: true);
        bazConfig.GetAppenders(LogLevel.Info).ShouldBe([appenderA], ignoreOrder: true);
        bazConfig.GetAppenders(LogLevel.Warn).ShouldBe([appenderA], ignoreOrder: true);
        bazConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA, appenderC], ignoreOrder: true);
        bazConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA, appenderC], ignoreOrder: true);
    }

    [Test]
    public void should_take_appender_level_into_account()
    {
        var appender = new NoopAppender { Level = LogLevel.Info };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = LogLevel.Debug,
                Appenders = { appender }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldBe(LogLevel.Info);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Warn).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Error).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appender], ignoreOrder: true);
    }

    [Test]
    public void should_take_appender_config_level_into_account()
    {
        var appender = new NoopAppender { Level = LogLevel.Info };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = LogLevel.Debug,
                Appenders =
                {
                    new AppenderConfiguration(appender)
                    {
                        Level = LogLevel.Warn
                    }
                }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldBe(LogLevel.Warn);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Warn).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Error).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appender], ignoreOrder: true);
    }

    [Test]
    public void should_not_override_appender_level()
    {
        var appender = new NoopAppender { Level = LogLevel.Warn };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = LogLevel.Debug,
                Appenders =
                {
                    new AppenderConfiguration(appender)
                    {
                        Level = LogLevel.Info
                    }
                }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldBe(LogLevel.Warn);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Warn).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Error).ShouldBe([appender], ignoreOrder: true);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appender], ignoreOrder: true);
    }

    [Test]
    public void should_use_parent_appenders_when_lowering_log_level()
    {
        var appenderA = new NoopAppender();
        var appenderB = new NoopAppender();

        var config = new ZeroLogConfiguration
        {
            Loggers =
            {
                new LoggerConfiguration("Foo")
                {
                    Level = LogLevel.Error,
                    Appenders = { appenderA }
                },
                new LoggerConfiguration("Foo.Bar")
                {
                    Level = LogLevel.Info,
                    Appenders = { appenderB }
                }
            }
        };

        var fooConfig = config.ResolveLoggerConfiguration("Foo");
        fooConfig.Level.ShouldBe(LogLevel.Error);
        fooConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA], ignoreOrder: true);
        fooConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA], ignoreOrder: true);

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldBe(LogLevel.Info);
        fooBarConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Info).ShouldBe([appenderA, appenderB], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Warn).ShouldBe([appenderA, appenderB], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Error).ShouldBe([appenderA, appenderB], ignoreOrder: true);
        fooBarConfig.GetAppenders(LogLevel.Fatal).ShouldBe([appenderA, appenderB], ignoreOrder: true);
    }

#if NET8_0_OR_GREATER
    [Test]
    public void should_forward_time_provider()
    {
        var timeProvider = new TestTimeProvider();

        var config = new ZeroLogConfiguration
        {
            TimeProvider = timeProvider
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.TimeProvider.ShouldBeSameAs(timeProvider);
    }
#endif
}
