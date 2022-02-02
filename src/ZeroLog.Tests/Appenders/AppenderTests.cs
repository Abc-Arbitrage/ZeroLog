using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
public class AppenderTests
{
    private FailingAppender _appender;
    private FormattedLogMessage _message;

    [SetUp]
    public void SetUp()
    {
        _appender = new FailingAppender();

        var logMessage = new LogMessage("Test");
        _message = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        _message.SetMessage(logMessage);
    }

    [Test]
    public void should_append()
    {
        _appender.Fail = false;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.AppendCount.ShouldEqual(1);

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.AppendCount.ShouldEqual(2);
    }

    [Test]
    public void should_not_throw_if_appender_implementation_throws()
    {
        _appender.Fail = true;

        Assert.DoesNotThrow(() => _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default));
    }

    [Test]
    public void should_disable_appender_if_it_throws()
    {
        _appender.Fail = true;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);

        _appender.AppendCount.ShouldEqual(1);
    }

    [Test]
    public async Task should_reenable_appender_after_quarantine_delay()
    {
        _appender.Fail = true;
        var config = new ZeroLogConfiguration { AppenderQuarantineDelay = TimeSpan.FromSeconds(1) };

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(1);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(1);

        await Task.Delay(TimeSpan.FromSeconds(1));

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(2);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(2);
    }

    private class FailingAppender : Appender
    {
        public bool Fail { get; set; }
        public int AppendCount { get; private set; }

        public override void WriteMessage(FormattedLogMessage message)
        {
            ++AppendCount;

            if (Fail)
                throw new InvalidOperationException();
        }
    }
}
