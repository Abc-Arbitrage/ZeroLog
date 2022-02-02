using System;
using Moq;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
public class GuardedAppenderTests
{
    private Mock<Appender> _appenderMock;
    private GuardedAppender _guardedAppender;

    [SetUp]
    public void SetUp()
    {
        _appenderMock = new Mock<Appender>();
        _guardedAppender = new GuardedAppender(_appenderMock.Object, TimeSpan.FromSeconds(15));
    }

    private static FormattedLogMessage GetFormattedMessage()
    {
        var logMessage = new LogMessage("Test");
        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);
        return formattedMessage;
    }

    [Test]
    public void should_append()
    {
        var message = GetFormattedMessage();

        _guardedAppender.WriteMessage(message);

        _appenderMock.Verify(x => x.WriteMessage(message), Times.Once);
    }

    [Test]
    public void should_not_throw_if_inner_appender_throws()
    {
        var message = GetFormattedMessage();

        _appenderMock.Setup(x => x.WriteMessage(message)).Throws<Exception>();

        Assert.DoesNotThrow(() => _guardedAppender.WriteMessage(message));
    }

    [Test]
    public void should_disable_appender_if_it_throws()
    {
        var message = GetFormattedMessage();

        _appenderMock.Setup(x => x.WriteMessage(message)).Throws<Exception>();

        _guardedAppender.WriteMessage(message);
        _guardedAppender.WriteMessage(message);

        _appenderMock.Verify(x => x.WriteMessage(message), Times.Once);
    }

    [Test]
    public void should_reenable_appender_after_quarantine_delay()
    {
        var message = GetFormattedMessage();

        _appenderMock.Setup(x => x.WriteMessage(message)).Throws<Exception>();

        SystemDateTime.PauseTime();
        _guardedAppender.WriteMessage(message);
        _appenderMock.Verify(x => x.WriteMessage(message), Times.Once);

        SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(2));
        _guardedAppender.WriteMessage(message);
        _appenderMock.Verify(x => x.WriteMessage(message), Times.Once);

        SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(20));
        _guardedAppender.WriteMessage(message);
        _appenderMock.Verify(x => x.WriteMessage(message), Times.Exactly(2));
    }
}
