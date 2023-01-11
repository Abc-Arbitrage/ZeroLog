using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Configuration;

[TestFixture]
public class LoggerConfigurationTests
{
    [Test]
    public void should_initialize_config_with_name()
    {
        var config = new LoggerConfiguration("Foo");
        config.Name.ShouldEqual("Foo");
        config.Level.ShouldBeNull();
    }

    [Test]
    public void should_initialize_config_with_name_and_level()
    {
        var config = new LoggerConfiguration("Foo", LogLevel.Info);
        config.Name.ShouldEqual("Foo");
        config.Level.ShouldEqual(LogLevel.Info);
    }

    [Test]
    public void should_initialize_config_with_type()
    {
        var config = new LoggerConfiguration(typeof(LoggerConfigurationTests));
        config.Name.ShouldEqual(typeof(LoggerConfigurationTests).FullName);
        config.Level.ShouldBeNull();
    }

    [Test]
    public void should_initialize_config_with_type_and_level()
    {
        var config = new LoggerConfiguration(typeof(LoggerConfigurationTests), LogLevel.Info);
        config.Name.ShouldEqual(typeof(LoggerConfigurationTests).FullName);
        config.Level.ShouldEqual(LogLevel.Info);
    }
}
