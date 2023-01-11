using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Configuration;

[TestFixture]
public class ZeroLogConfigurationTests
{
    [Test]
    public void should_set_log_level()
    {
        var config = new ZeroLogConfiguration();
        config.Loggers.ShouldBeEmpty();

        config.SetLogLevel("Foo", LogLevel.Info);

        var loggerConfig = config.Loggers.ShouldHaveSingleItem();
        loggerConfig.Name.ShouldEqual("Foo");
        loggerConfig.Level.ShouldEqual(LogLevel.Info);

        config.SetLogLevel("Foo", LogLevel.Warn);
        config.Loggers.ShouldHaveSingleItem().ShouldBeTheSameAs(loggerConfig);
        loggerConfig.Level.ShouldEqual(LogLevel.Warn);

        config.SetLogLevel("Foo", null);
        loggerConfig.Level.ShouldBeNull();
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void should_set_root_log_level(string name)
    {
        var config = new ZeroLogConfiguration();
        config.Loggers.ShouldBeEmpty();

        config.SetLogLevel(name, LogLevel.Warn);

        config.RootLogger.Level.ShouldEqual(LogLevel.Warn);
        config.Loggers.ShouldBeEmpty();
    }
}
