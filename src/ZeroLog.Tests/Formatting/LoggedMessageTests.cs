using System;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class LoggedMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 16;

    private LogMessage _logMessage;

    static LoggedMessageTests()
    {
        LogManager.RegisterEnum<DayOfWeek>();
    }

    [SetUp]
    public void SetUp()
    {
        _logMessage = LogMessage.CreateTestMessage(LogLevel.Info, _bufferLength, _stringCapacity);
    }

    [Test]
    public void should_format_message()
    {
        _logMessage.Append("Foo").Append("Bar");
        GetFormatted().ToString().ShouldEqual("FooBar");
    }

    [Test]
    public void should_clone_message()
    {
        _logMessage.Append("Foo")
                   .Append("Bar")
                   .AppendKeyValue("Fizz", "Buzz")
                   .AppendKeyValue("Hello", "World")
                   .WithException(new InvalidOperationException("Oops"));

        var original = GetFormatted();
        var clone = original.Clone();

        clone.ShouldNotBeTheSameAs(original);
        clone.Level.ShouldEqual(original.Level);
        clone.Timestamp.ShouldEqual(original.Timestamp);
        clone.Thread.ShouldEqual(original.Thread);
        clone.Exception.ShouldEqual(original.Exception);
        clone.LoggerName.ShouldEqual(original.LoggerName);
        clone.Message.ToString().ShouldEqual(original.Message.ToString());

        clone.KeyValues.ShouldNotBeTheSameAs(original.KeyValues);
        clone.KeyValues.Count.ShouldEqual(original.KeyValues.Count);

        for (var i = 0; i < original.KeyValues.Count; ++i)
        {
            clone.KeyValues[i].Key.ShouldEqual(original.KeyValues[i].Key);
            clone.KeyValues[i].Value.ToString().ShouldEqual(original.KeyValues[i].Value.ToString());
        }
    }

    [Test]
    public void should_keep_cloned_data_after_reset()
    {
        _logMessage.Append("Foo")
                   .Append("Bar")
                   .AppendKeyValue("Fizz", "Buzz")
                   .AppendKeyValue("Hello", "World")
                   .WithException(new InvalidOperationException("Oops"));

        var original = GetFormatted();
        var clone = original.Clone();

        var dataBeforeReset = (
            clone.Level,
            clone.Timestamp,
            clone.Thread,
            clone.Exception,
            clone.LoggerName,
            clone.Message.ToString(),
            clone.KeyValues.Count
        );

        _logMessage.Initialize(null, LogLevel.Warn);
        original.SetMessage(LogMessage.CreateTestMessage(LogLevel.Warn, 0, 0));

        var dataAfterReset = (
            clone.Level,
            clone.Timestamp,
            clone.Thread,
            clone.Exception,
            clone.LoggerName,
            clone.Message.ToString(),
            clone.KeyValues.Count
        );

        dataAfterReset.ShouldEqual(dataBeforeReset);
    }

    private LoggedMessage GetFormatted()
    {
        var message = new LoggedMessage(_bufferLength, ZeroLogConfiguration.Default);
        message.SetMessage(_logMessage);
        return message;
    }
}
