using NUnit.Framework;
using Shouldly;
using ZeroLog.Configuration;
namespace ZeroLog.Tests.Configuration;

[TestFixture]
public class LoggerConfigurationTests
{
    [Test]
    public void should_initialize_config_with_name()
    {
        var config = new LoggerConfiguration("Foo");
        config.Name.ShouldBe("Foo");
        config.Level.ShouldBeNull();
    }

    [Test]
    public void should_initialize_config_with_name_and_level()
    {
        var config = new LoggerConfiguration("Foo", LogLevel.Info);
        config.Name.ShouldBe("Foo");
        config.Level.ShouldBe(LogLevel.Info);
    }

    [Test]
    public void should_initialize_config_with_type()
    {
        var config = new LoggerConfiguration(typeof(LoggerConfigurationTests));
        config.Name.ShouldBe(typeof(LoggerConfigurationTests).FullName);
        config.Level.ShouldBeNull();
    }

    [Test]
    public void should_initialize_config_with_type_and_level()
    {
        var config = new LoggerConfiguration(typeof(LoggerConfigurationTests), LogLevel.Info);
        config.Name.ShouldBe(typeof(LoggerConfigurationTests).FullName);
        config.Level.ShouldBe(LogLevel.Info);
    }
}
