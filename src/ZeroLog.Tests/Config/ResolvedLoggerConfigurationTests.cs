using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Config;

[TestFixture]
public class ResolvedLoggerConfigurationTests
{
    [Test]
    public void should_have_empty_config()
    {
        var emptyConfig = ResolvedLoggerConfiguration.Empty;
        emptyConfig.Level.ShouldEqual(Level.None);
        emptyConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.DropLogMessage);
        emptyConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        emptyConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        emptyConfig.GetAppenders(Level.Info).ShouldBeEmpty();
        emptyConfig.GetAppenders(Level.Warn).ShouldBeEmpty();
        emptyConfig.GetAppenders(Level.Error).ShouldBeEmpty();
        emptyConfig.GetAppenders(Level.Fatal).ShouldBeEmpty();
    }

    [Test]
    public void should_resolve_configuration()
    {
        var appenderA = new NoopAppender();
        var appenderB = new NoopAppender();
        var appenderC = new NoopAppender { Level = Level.Warn };
        var appenderD = new NoopAppender();

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = Level.Debug,
                Appenders = { appenderA }
            },
            Loggers =
            {
                new LoggerConfiguration("Foo")
                {
                    Level = Level.Error,
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                    Appenders = { appenderB }
                },
                new LoggerConfiguration("Foo.Bar")
                {
                    Level = Level.Info,
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
                            Level = Level.Error
                        }
                    }
                }
            }
        };

        var rootConfig = config.ResolveLoggerConfiguration("SomethingElse");
        rootConfig.Level.ShouldEqual(Level.Debug);
        rootConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.Default);
        rootConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        rootConfig.GetAppenders(Level.Debug).ShouldBeEquivalentTo(new[] { appenderA });
        rootConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appenderA });
        rootConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appenderA });
        rootConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA });
        rootConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA });

        var fooConfig = config.ResolveLoggerConfiguration("Foo");
        fooConfig.Level.ShouldEqual(Level.Error);
        fooConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
        fooConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA, appenderB });

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldEqual(Level.Info);
        fooBarConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
        fooBarConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appenderA, appenderB, appenderC });
        fooBarConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA, appenderB, appenderC });
        fooBarConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA, appenderB, appenderC });

        var fooBarClearConfig = config.ResolveLoggerConfiguration("Foo.Bar.Clear");
        fooBarClearConfig.Level.ShouldEqual(Level.Info);
        fooBarClearConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);
        fooBarClearConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        fooBarClearConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appenderD });
        fooBarClearConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appenderD });
        fooBarClearConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderD });
        fooBarClearConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderD });

        var bazConfig = config.ResolveLoggerConfiguration("Baz");
        bazConfig.Level.ShouldEqual(Level.Debug);
        bazConfig.LogMessagePoolExhaustionStrategy.ShouldEqual(LogMessagePoolExhaustionStrategy.Default);
        bazConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        bazConfig.GetAppenders(Level.Debug).ShouldBeEquivalentTo(new[] { appenderA });
        bazConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appenderA });
        bazConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appenderA });
        bazConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA, appenderC });
        bazConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA, appenderC });
    }

    [Test]
    public void should_take_appender_level_into_account()
    {
        var appender = new NoopAppender { Level = Level.Info };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = Level.Debug,
                Appenders = { appender }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldEqual(Level.Info);
        logConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appender });
    }

    [Test]
    public void should_take_appender_config_level_into_account()
    {
        var appender = new NoopAppender { Level = Level.Info };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = Level.Debug,
                Appenders =
                {
                    new AppenderConfiguration(appender)
                    {
                        Level = Level.Warn
                    }
                }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldEqual(Level.Warn);
        logConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Info).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appender });
    }

    [Test]
    public void should_not_override_appender_level()
    {
        var appender = new NoopAppender { Level = Level.Warn };

        var config = new ZeroLogConfiguration
        {
            RootLogger =
            {
                Level = Level.Debug,
                Appenders =
                {
                    new AppenderConfiguration(appender)
                    {
                        Level = Level.Info
                    }
                }
            }
        };

        var logConfig = config.ResolveLoggerConfiguration("Foo");
        logConfig.Level.ShouldEqual(Level.Warn);
        logConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Info).ShouldBeEmpty();
        logConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appender });
        logConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appender });
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
                    Level = Level.Error,
                    Appenders = { appenderA }
                },
                new LoggerConfiguration("Foo.Bar")
                {
                    Level = Level.Info,
                    Appenders = { appenderB }
                }
            }
        };

        var fooConfig = config.ResolveLoggerConfiguration("Foo");
        fooConfig.Level.ShouldEqual(Level.Error);
        fooConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Info).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Warn).ShouldBeEmpty();
        fooConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA });
        fooConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA });

        var fooBarConfig = config.ResolveLoggerConfiguration("Foo.Bar");
        fooBarConfig.Level.ShouldEqual(Level.Info);
        fooBarConfig.GetAppenders(Level.Trace).ShouldBeEmpty();
        fooBarConfig.GetAppenders(Level.Debug).ShouldBeEmpty();
        fooBarConfig.GetAppenders(Level.Info).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
        fooBarConfig.GetAppenders(Level.Warn).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
        fooBarConfig.GetAppenders(Level.Error).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
        fooBarConfig.GetAppenders(Level.Fatal).ShouldBeEquivalentTo(new[] { appenderA, appenderB });
    }
}
