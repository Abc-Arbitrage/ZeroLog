using NUnit.Framework;
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
        emptyConfig.Level.ShouldEqual(LogLevel.None);
        emptyConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.DropLogMessage);
        emptyConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Error).ShouldBeEmpty();
        emptyConfig.GetAppenders(LogLevel.Fatal).ShouldBeEmpty();
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
        rootConfig.Level.ShouldEqual(LogLevel.Debug);
        rootConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.Default);
        rootConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        rootConfig.GetAppenders(LogLevel.Debug).ShouldBeEquivalentTo([appenderA]);
        rootConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appenderA]);
        rootConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appenderA]);
        rootConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA]);
        rootConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA]);

        var fooConfig = config.ResolveLoggerConfiguration("Foo");
        fooConfig.Level.ShouldEqual(LogLevel.Error);
        fooConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA, appenderB]);
        fooConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA, appenderB]);

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldEqual(LogLevel.Info);
        fooBarConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appenderA, appenderB]);
        fooBarConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appenderA, appenderB, appenderC]);
        fooBarConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA, appenderB, appenderC]);
        fooBarConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA, appenderB, appenderC]);

        var fooBarClearConfig = config.ResolveLoggerConfiguration("Foo.Bar.Clear");
        fooBarClearConfig.Level.ShouldEqual(LogLevel.Info);
        fooBarClearConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarClearConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appenderD]);
        fooBarClearConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appenderD]);
        fooBarClearConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderD]);
        fooBarClearConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderD]);

        var bazConfig = config.ResolveLoggerConfiguration("Baz");
        bazConfig.Level.ShouldEqual(LogLevel.Debug);
        bazConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.Default);
        bazConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        bazConfig.GetAppenders(LogLevel.Debug).ShouldBeEquivalentTo([appenderA]);
        bazConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appenderA]);
        bazConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appenderA]);
        bazConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA, appenderC]);
        bazConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA, appenderC]);
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
        logConfig.Level.ShouldEqual(LogLevel.Info);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appender]);
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
        logConfig.Level.ShouldEqual(LogLevel.Warn);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appender]);
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
        logConfig.Level.ShouldEqual(LogLevel.Warn);
        logConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        logConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appender]);
        logConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appender]);
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
        fooConfig.Level.ShouldEqual(LogLevel.Error);
        fooConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA]);
        fooConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA]);

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldEqual(LogLevel.Info);
        fooBarConfig.GetAppenders(LogLevel.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(LogLevel.Info).ShouldBeEquivalentTo([appenderA, appenderB]);
        fooBarConfig.GetAppenders(LogLevel.Warn).ShouldBeEquivalentTo([appenderA, appenderB]);
        fooBarConfig.GetAppenders(LogLevel.Error).ShouldBeEquivalentTo([appenderA, appenderB]);
        fooBarConfig.GetAppenders(LogLevel.Fatal).ShouldBeEquivalentTo([appenderA, appenderB]);
    }
}
