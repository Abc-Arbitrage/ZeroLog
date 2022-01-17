using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests
{
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
            BasicConfigurator.Configure(new List<Appender> { _testAppender }, new ZeroLogInitializationConfig { LogMessagePoolSize = 10 });
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

            Check.That(log).IsNotNull();
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
            Assert.Throws<ApplicationException>(() => BasicConfigurator.Configure(new Appender[0]));
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

            Check.That(actualLogMessages.Count).Equals(actualLogMessages.Count);
            Check.That(unavailableEvent.ConstantMessage).IsNotNull();

            var signal = _testAppender.SetMessageCountTarget(actualLogMessages.Count);

            for (var i = 0; i < actualLogMessages.Count; i++)
            {
                var actualLogMessage = actualLogMessages[i];
                actualLogMessage.Append(i).Log();
            }

            signal.Wait(TimeSpan.FromMilliseconds(100));

            Check.That(log.Debug().ConstantMessage).IsNull();
        }

        [Test]
        public void should_log_special_message_when_log_event_pool_is_exhausted()
        {
            LogManager.Shutdown();

            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _testAppender },
                LogMessagePoolSize = 10,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders
            });

            var log = LogManager.GetLogger(typeof(LogManagerTests));

            for (var i = 0; i < 10; i++)
                log.Debug();

            var signal = _testAppender.SetMessageCountTarget(1);

            log.Debug().Append("this is not going to happen").Log();

            Check.That(signal.Wait(TimeSpan.FromMilliseconds(100))).IsTrue();

            Check.That(_testAppender.LoggedMessages.Last()).Contains("Log message skipped due to pool exhaustion.");
        }

        [Test]
        public void should_completely_drop_log_event_when_log_event_pool_is_exhausted()
        {
            LogManager.Shutdown();

            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _testAppender },
                LogMessagePoolSize = 10,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage
            });

            var log = LogManager.GetLogger(typeof(LogManagerTests));

            for (var i = 0; i < 10; i++)
                log.Debug();

            var signal = _testAppender.SetMessageCountTarget(1);

            log.Debug().Append("this is not going to happen").Log();

            Check.That(signal.Wait(TimeSpan.FromMilliseconds(100))).IsFalse();
        }

        [Test]
        public void should_wait_for_event_when_log_event_pool_is_exhausted()
        {
            LogManager.Shutdown();

            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _testAppender },
                LogMessagePoolSize = 10,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable
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

            Check.That(logCompletedSignal.WaitOne(TimeSpan.FromMilliseconds(100))).IsFalse();

            actualLogMessages[0].Log();

            Check.That(logCompletedSignal.WaitOne(TimeSpan.FromMilliseconds(100))).IsTrue();
            Check.That(signal.Wait(TimeSpan.FromMilliseconds(100))).IsTrue();
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

            signal.Wait(TimeSpan.FromMilliseconds(100));

            var logMessage = _testAppender.LoggedMessages.Single();
            logMessage.ShouldContain("An error occured during formatting:");
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
               // .AppendUnmanaged(new FailingUnmanagedStruct { Value = 42 }) // TODO
               .Log();

            signal.Wait(TimeSpan.FromMilliseconds(100));

            var logMessage = _testAppender.LoggedMessages.Single();
            // Check.That(logMessage).Equals("An error occured during formatting: Simulated failure - Arguments: Unmanaged(0x2a000000)"); // TODO
        }

        [Test]
        public void should_flush_appenders_when_not_logging_messages()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));
            var signal = _testAppender.SetMessageCountTarget(3);
            _testAppender.WaitOnWriteEvent = new ManualResetEventSlim(false);

            log.Info($"Foo");
            log.Info($"Bar");
            log.Info($"Baz");

            _testAppender.WaitOnWriteEvent.Set();
            signal.Wait(TimeSpan.FromMilliseconds(500));

            Wait.Until(() => _testAppender.FlushCount == 1, TimeSpan.FromSeconds(1));

            log.Info($"Foo");
            Wait.Until(() => _testAppender.FlushCount == 2, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void should_register_all_assembly_enums()
        {
            Check.That(EnumCache.IsRegistered(typeof(ConsoleColor))).IsFalse();
            LogManager.RegisterAllEnumsFrom(typeof(ConsoleColor).Assembly);
            Check.That(EnumCache.IsRegistered(typeof(ConsoleColor))).IsTrue();
        }

        [Test]
        public void should_truncate_long_lines()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var signal = _testAppender.SetMessageCountTarget(1);

            var longMessage = new string('.', LogManager.OutputBufferSize + 1);
            log.Info().Append(longMessage).Log();

            signal.Wait(TimeSpan.FromMilliseconds(100));
            var message = _testAppender.LoggedMessages.Single();
            Check.That(message).IsEqualTo(new string('.', LogManager.OutputBufferSize - LogManager.Config.TruncatedMessageSuffix.Length) + LogManager.Config.TruncatedMessageSuffix);
        }

        public struct FailingUnmanagedStruct : ISpanFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
                => throw new InvalidOperationException("Simulated failure");

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
                => throw new InvalidOperationException("Simulated failure");
        }
    }
}
