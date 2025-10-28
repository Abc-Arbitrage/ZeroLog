using System;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
[Parallelizable(ParallelScope.All)]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class AppenderTests
{
    private FailingAppender _appender;
    private LoggedMessage _message;

    [SetUp]
    public void SetUp()
    {
        _appender = new FailingAppender();

        var logMessage = new LogMessage("Test");
        _message = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        _message.SetMessage(logMessage);
    }

    [Test]
    public void should_append()
    {
        _appender.FailOnAppend = false;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.AppendCount.ShouldEqual(1);

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.AppendCount.ShouldEqual(2);
    }

    [Test]
    public void test_messages_should_work_correctly()
    {
        var realMessage = new LogMessage(BufferSegmentProvider.CreateStandaloneSegment(1024), 16);
        realMessage.Append("Hello, World! 42 is the answer.")
                   .AppendKeyValue("Answer", 42);

        var realLoggedMessage = new LoggedMessage(1024, ZeroLogConfiguration.Default);
        realLoggedMessage.SetMessage(realMessage);

        var testMessage = LogMessage.CreateTestMessage(LogLevel.Info, 1024, 16);
        testMessage.Append("Hello, World! 42 is the answer.")
                   .AppendKeyValue("Answer", 42);

        var testLoggedMessage = LoggedMessage.CreateTestMessage(testMessage, ZeroLogConfiguration.Default);

        testLoggedMessage.Message.ToString().ShouldEqual(realLoggedMessage.Message.ToString());
        var (realKey, realValue) = realLoggedMessage.KeyValues[0];
        var (testKey, testValue) = testLoggedMessage.KeyValues[0];

        testKey.ShouldEqual(realKey);
        testValue.SequenceEqual(realValue).ShouldBeTrue();
    }

    [Test]
    public void should_not_throw_if_appender_implementation_throws()
    {
        _appender.FailOnAppend = true;

        Assert.DoesNotThrow(() => _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default));
    }

    [Test]
    public void should_disable_appender_if_it_throws()
    {
        _appender.FailOnAppend = true;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);

        _appender.AppendCount.ShouldEqual(1);
    }

    [Test]
    public async Task should_reenable_appender_after_quarantine_delay()
    {
        _appender.FailOnAppend = true;
        var config = new ZeroLogConfiguration { AppenderQuarantineDelay = TimeSpan.FromSeconds(1) };

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(1);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(1);

        await Task.Delay(TimeSpan.FromSeconds(2));

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(2);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(2);
    }

    [Test]
    public void should_not_flush_unnecessarily()
    {
        _appender.InternalFlush();
        _appender.FlushCount.ShouldEqual(0);

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.FlushCount.ShouldEqual(0);

        _appender.InternalFlush();
        _appender.FlushCount.ShouldEqual(1);

        _appender.InternalFlush();
        _appender.FlushCount.ShouldEqual(1);
    }

    [Test]
    public void should_not_throw_on_flush_if_appender_implementation_throws()
    {
        _appender.FailOnFlush = true;
        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);

        Assert.DoesNotThrow(() => _appender.InternalFlush());
    }

    [Test]
    public void should_disable_appender_if_it_throws_on_flush()
    {
        _appender.FailOnFlush = true;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalFlush();

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalFlush();

        _appender.AppendCount.ShouldEqual(1);
        _appender.FlushCount.ShouldEqual(1);
    }

    [Test]
    public async Task should_reenable_appender_after_quarantine_delay_due_to_flush()
    {
        _appender.FailOnFlush = true;
        var config = new ZeroLogConfiguration { AppenderQuarantineDelay = TimeSpan.FromSeconds(1) };

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldEqual(1);
        _appender.FlushCount.ShouldEqual(1);

        await Task.Delay(TimeSpan.FromSeconds(2));

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();
        _appender.AppendCount.ShouldEqual(2);
        _appender.FlushCount.ShouldEqual(2);

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();
        _appender.AppendCount.ShouldEqual(2);
        _appender.FlushCount.ShouldEqual(2);
    }

    private class FailingAppender : Appender
    {
        public bool FailOnAppend { get; set; }
        public bool FailOnFlush { get; set; }

        public int AppendCount { get; private set; }
        public int FlushCount { get; private set; }

        public override void WriteMessage(LoggedMessage message)
        {
            ++AppendCount;

            if (FailOnAppend)
                throw new InvalidOperationException();
        }

        public override void Flush()
        {
            ++FlushCount;

            if (FailOnFlush)
                throw new InvalidOperationException();
        }
    }
}
