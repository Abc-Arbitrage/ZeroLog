using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture, NonParallelizable]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public class LogManagerTests
{
    private TestAppender _testAppender;

    [SetUp]
    public void SetUpFixture()
    {
        _testAppender = new TestAppender(true);

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            RootLogger =
            {
                Appenders = { _testAppender }
            }
        });
    }

    [TearDown]
    public void Teardown()
    {
        LogManager.Shutdown();
    }

    [Test]
    public void should_create_log()
    {
        var log = LogManager.GetLogger<LogManagerTests>();

        log.ShouldNotBeNull();
    }

    [Test]
    public void should_ignore_logs_after_shutdown()
    {
        var log = LogManager.GetLogger<LogManagerTests>();
        LogManager.Shutdown();

        log.Info().Append("Hello").Log();
    }

    [Test]
    public void should_prevent_initializing_already_initialized_log_manager()
    {
        Assert.Throws<InvalidOperationException>(() => LogManager.Initialize(new ZeroLogConfiguration()));
    }

    [Test]
    public void should_return_special_log_event_when_no_more_log_event_are_available()
    {
        var log = LogManager.GetLogger(typeof(LogManagerTests));

        var actualLogMessages = new List<LogMessage>();
        for (var i = 0; i < 10; i++)
        {
            actualLogMessages.Add(log.Debug());
        }

        var unavailableEvent = log.Debug();

        actualLogMessages.Count.ShouldEqual(actualLogMessages.Count);
        unavailableEvent.ConstantMessage.ShouldNotBeNull();

        var signal = _testAppender.SetMessageCountTarget(actualLogMessages.Count);

        for (var i = 0; i < actualLogMessages.Count; i++)
        {
            var actualLogMessage = actualLogMessages[i];
            actualLogMessage.Append(i).Log();
        }

        signal.Wait(TimeSpan.FromSeconds(1));

        log.Debug().ConstantMessage.ShouldBeNull();
    }

    [Test]
    public void should_log_special_message_when_log_event_pool_is_exhausted()
    {
        LogManager.Shutdown();

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders,
                Appenders = { _testAppender }
            }
        });

        var log = LogManager.GetLogger(typeof(LogManagerTests));

        for (var i = 0; i < 10; i++)
            log.Debug();

        var signal = _testAppender.SetMessageCountTarget(1);

        log.Debug().Append("this is not going to happen").Log();

        signal.Wait(TimeSpan.FromSeconds(1)).ShouldBeTrue();

        _testAppender.LoggedMessages.Last().ShouldContain("Log message skipped due to pool exhaustion.");
    }

    [Test]
    public void should_completely_drop_log_event_when_log_event_pool_is_exhausted()
    {
        LogManager.Shutdown();

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage,
                Appenders = { _testAppender }
            }
        });

        var log = LogManager.GetLogger(typeof(LogManagerTests));

        for (var i = 0; i < 10; i++)
            log.Debug();

        var signal = _testAppender.SetMessageCountTarget(1);

        log.Debug().Append("this is not going to happen").Log();

        signal.Wait(TimeSpan.FromSeconds(1)).ShouldBeFalse();
    }

    [Test]
    public void should_wait_for_event_when_log_event_pool_is_exhausted()
    {
        LogManager.Shutdown();

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = 10,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                Appenders = { _testAppender }
            }
        });

        var log = LogManager.GetLogger(typeof(LogManagerTests));

        var actualLogMessages = new List<LogMessage>();
        for (var i = 0; i < 10; i++)
            actualLogMessages.Add(log.Debug());

        var signal = _testAppender.SetMessageCountTarget(2);
        var logCompletedSignal = new ManualResetEvent(false);

        Task.Run(() =>
        {
            log.Debug().Append("this is not going to happen").Log();
            logCompletedSignal.Set();
        });

        logCompletedSignal.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeFalse();

        actualLogMessages[0].Log();

        logCompletedSignal.WaitOne(TimeSpan.FromSeconds(1)).ShouldBeTrue();
        signal.Wait(TimeSpan.FromSeconds(1)).ShouldBeTrue();
    }

    [Test]
    public void should_not_throw_if_formatting_fails_when_appending_formatted_arguments()
    {
        LogManager.RegisterEnum<DayOfWeek>();
        var log = LogManager.GetLogger(typeof(LogManagerTests));
        var signal = _testAppender.SetMessageCountTarget(1);

        var guid = Guid.NewGuid();
        var date = new DateTime(2017, 02, 24, 16, 51, 51);
        var timespan = date.TimeOfDay;

        log.Info()
           .Append("Hello")
           .Append(false)
           .Append((byte)1)
           .Append('a')
           .Append((short)2)
           .Append(3)
           .Append((long)4)
           .Append(5f)
           .Append(6d)
           .Append(7m)
           .Append(guid, "meh, this is going to break formatting")
           .Append(date)
           .Append(timespan)
           .AppendAsciiString(new[] { (byte)'a', (byte)'b', (byte)'c' })
           .AppendEnum(DayOfWeek.Friday)
           .Log();

        signal.Wait(TimeSpan.FromSeconds(1));

        var logMessage = _testAppender.LoggedMessages.Single();
        logMessage.ShouldContain("An error occurred during formatting:");
        logMessage.ShouldContain(guid.ToString(null, CultureInfo.InvariantCulture));
        logMessage.ShouldContain("abc");
        logMessage.ShouldContain(nameof(DayOfWeek.Friday));
    }

    [Test]
    public void should_write_unformatted_unmanaged_struct_when_formatting_fails()
    {
        LogManager.RegisterUnmanaged<FailingUnmanagedStruct>();

        var log = LogManager.GetLogger(typeof(LogManagerTests));
        var signal = _testAppender.SetMessageCountTarget(1);

        log.Info()
           .AppendUnmanaged(new FailingUnmanagedStruct { Value = 42 })
           .Log();

        signal.Wait(TimeSpan.FromSeconds(1));

        var logMessage = _testAppender.LoggedMessages.Single();
        logMessage.ShouldEqual("An error occurred during formatting: Simulated failure - Unformatted message: Unmanaged(0x2a000000)");
    }

    [Test]
    public void should_flush_appenders_when_not_logging_messages()
    {
        var log = LogManager.GetLogger(typeof(LogManagerTests));
        var signal = _testAppender.SetMessageCountTarget(3);
        _testAppender.WaitOnWriteEvent = new ManualResetEventSlim(false);

        log.Info("Foo");
        log.Info("Bar");
        log.Info("Baz");

        _testAppender.WaitOnWriteEvent.Set();
        signal.Wait(TimeSpan.FromSeconds(1));

        Wait.Until(() => _testAppender.FlushCount == 1, TimeSpan.FromSeconds(1));

        log.Info("Foo");
        Wait.Until(() => _testAppender.FlushCount == 2, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void should_register_all_assembly_enums()
    {
        EnumCache.IsRegistered(typeof(ConsoleColor)).ShouldBeFalse();
        LogManager.RegisterAllEnumsFrom(typeof(ConsoleColor).Assembly);
        EnumCache.IsRegistered(typeof(ConsoleColor)).ShouldBeTrue();
    }

    [Test]
    public void should_truncate_long_lines()
    {
        var log = LogManager.GetLogger(typeof(LogManagerTests));

        var signal = _testAppender.SetMessageCountTarget(1);

        var longMessage = new string('.', LogManager.OutputBufferSize + 1);
        log.Info().Append(longMessage).Log();

        signal.Wait(TimeSpan.FromSeconds(1));
        var message = _testAppender.LoggedMessages.Single();
        message.ShouldEqual(new string('.', LogManager.OutputBufferSize - ZeroLogConfiguration.Default.TruncatedMessageSuffix.Length) + ZeroLogConfiguration.Default.TruncatedMessageSuffix);
    }

    public struct FailingUnmanagedStruct : ISpanFormattable
    {
        public int Value;

        public string ToString(string format, IFormatProvider formatProvider)
            => throw new InvalidOperationException("Simulated failure");

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
            => throw new InvalidOperationException("Simulated failure");
    }
}
