using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

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
        _appender.AppendCount.ShouldBe(1);

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.AppendCount.ShouldBe(2);
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

        testLoggedMessage.Message.ToString().ShouldBe(realLoggedMessage.Message.ToString());
        var (realKey, realValue) = realLoggedMessage.KeyValues[0];
        var (testKey, testValue) = testLoggedMessage.KeyValues[0];

        testKey.ShouldBe(realKey);
        testValue.SequenceEqual(realValue).ShouldBeTrue();
    }

    [Test]
    public void should_not_throw_if_appender_implementation_throws()
    {
        _appender.FailOnAppend = true;

        Should.NotThrow(() => _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default));
    }

    [Test]
    public void should_disable_appender_if_it_throws()
    {
        _appender.FailOnAppend = true;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);

        _appender.AppendCount.ShouldBe(1);
    }

    [Test]
    public async Task should_reenable_appender_after_quarantine_delay()
    {
        _appender.FailOnAppend = true;
        var config = new ZeroLogConfiguration { AppenderQuarantineDelay = TimeSpan.FromSeconds(1) };

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldBe(1);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldBe(1);

        await Task.Delay(TimeSpan.FromSeconds(2));

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldBe(2);

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldBe(2);
    }

    [Test]
    public void should_not_flush_unnecessarily()
    {
        _appender.InternalFlush();
        _appender.FlushCount.ShouldBe(0);

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.FlushCount.ShouldBe(0);

        _appender.InternalFlush();
        _appender.FlushCount.ShouldBe(1);

        _appender.InternalFlush();
        _appender.FlushCount.ShouldBe(1);
    }

    [Test]
    public void should_not_throw_on_flush_if_appender_implementation_throws()
    {
        _appender.FailOnFlush = true;
        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);

        Should.NotThrow(() => _appender.InternalFlush());
    }

    [Test]
    public void should_disable_appender_if_it_throws_on_flush()
    {
        _appender.FailOnFlush = true;

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalFlush();

        _appender.InternalWriteMessage(_message, ZeroLogConfiguration.Default);
        _appender.InternalFlush();

        _appender.AppendCount.ShouldBe(1);
        _appender.FlushCount.ShouldBe(1);
    }

    [Test]
    public async Task should_reenable_appender_after_quarantine_delay_due_to_flush()
    {
        _appender.FailOnFlush = true;
        var config = new ZeroLogConfiguration { AppenderQuarantineDelay = TimeSpan.FromSeconds(1) };

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();

        _appender.InternalWriteMessage(_message, config);
        _appender.AppendCount.ShouldBe(1);
        _appender.FlushCount.ShouldBe(1);

        await Task.Delay(TimeSpan.FromSeconds(2));

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();
        _appender.AppendCount.ShouldBe(2);
        _appender.FlushCount.ShouldBe(2);

        _appender.InternalWriteMessage(_message, config);
        _appender.InternalFlush();
        _appender.AppendCount.ShouldBe(2);
        _appender.FlushCount.ShouldBe(2);
    }

    [Test]
    public void should_initialize_once()
    {
        _appender.InitializationCount.ShouldBe(0);
        _appender.InternalInitialize();
        _appender.InitializationCount.ShouldBe(1);
        _appender.InternalInitialize();
        _appender.InitializationCount.ShouldBe(1);
    }

    [Test]
    public void should_not_throw_if_appender_initialization_throws()
    {
        _appender.FailOnInitialize = true;

        Should.NotThrow(() => _appender.InternalInitialize());
    }

    private class FailingAppender : Appender
    {
        public bool FailOnInitialize { get; set; }
        public bool FailOnAppend { get; set; }
        public bool FailOnFlush { get; set; }

        public int InitializationCount { get; private set; }
        public int AppendCount { get; private set; }
        public int FlushCount { get; private set; }

        public override void Initialize()
        {
            ++InitializationCount;

            if (FailOnInitialize)
                throw new InvalidOperationException();
        }

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
