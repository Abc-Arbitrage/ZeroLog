using System;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class FormattedLogMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 16;

    private LogMessage _logMessage;

    static FormattedLogMessageTests()
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

    private LoggedMessage GetFormatted()
    {
        var message = new LoggedMessage(_bufferLength, ZeroLogConfiguration.Default);
        message.SetMessage(_logMessage);
        return message;
    }
}
