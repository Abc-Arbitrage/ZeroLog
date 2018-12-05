using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class LogManagerTests
    {
        private TestAppender _testAppender;

        [SetUp]
        public void SetUpFixture()
        {
            _testAppender = new TestAppender(true);
            BasicConfigurator.Configure(new List<IAppender> { _testAppender }, new ZeroLogInitializationConfig { LogEventQueueSize = 10 });
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
        public void should_prevent_initializing_already_initialized_log_manager()
        {
            Assert.Throws<ApplicationException>(() => BasicConfigurator.Configure(new IAppender[0]));
        }

        [Test]
        public void should_return_special_log_event_when_no_more_log_event_are_available()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

            var unavailableEvent = log.Debug();

            Check.That(actualLogEvents.OfType<LogEvent>().Count()).Equals(actualLogEvents.Count);
            Check.That(unavailableEvent).IsInstanceOf<ForwardingLogEvent>();

            var signal = _testAppender.SetMessageCountTarget(actualLogEvents.Count);

            for (var i = 0; i < actualLogEvents.Count; i++)
            {
                var actualLogEvent = actualLogEvents[i];
                actualLogEvent.Append(i).Log();
            }

            signal.Wait(TimeSpan.FromMilliseconds(100));

            Check.That(log.Debug()).IsInstanceOf<LogEvent>();
        }

        [Test]
        public void should_log_special_message_when_log_event_pool_is_exhausted()
        {
            LogManager.Shutdown();

            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _testAppender },
                LogEventQueueSize = 10,
                LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessageAndNotifyAppenders
            });

            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

            var signal = _testAppender.SetMessageCountTarget(1);

            log.Debug().Append("this is not going to happen").Log();

            Check.That(signal.Wait(TimeSpan.FromMilliseconds(100))).IsTrue();

            Check.That(_testAppender.LoggedMessages.Last()).Contains("Log message skipped due to LogEvent pool exhaustion.");
        }

        [Test]
        public void should_completely_drop_log_event_when_log_event_pool_is_exhausted()
        {
            LogManager.Shutdown();
            
            BasicConfigurator.Configure(new ZeroLogBasicConfiguration
            {
                Appenders = { _testAppender },
                LogEventQueueSize = 10,
                LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessage
            });

            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

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
                LogEventQueueSize = 10,
                LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.WaitForLogEvent
            });

            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

            var signal = _testAppender.SetMessageCountTarget(2);
            var logCompletedSignal = new ManualResetEvent(false);

            Task.Run(() =>
            {
                log.Debug().Append("this is not going to happen").Log();
                logCompletedSignal.Set();
            });

            Check.That(logCompletedSignal.WaitOne(TimeSpan.FromMilliseconds(100))).IsFalse();

            actualLogEvents[0].Log();

            Check.That(logCompletedSignal.WaitOne(TimeSpan.FromMilliseconds(100))).IsTrue();
            Check.That(signal.Wait(TimeSpan.FromMilliseconds(100))).IsTrue();
        }

        [Test]
        public void should_not_throw_if_formatting_fails_when_using_format_string()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));
            var signal = _testAppender.SetMessageCountTarget(1);

            var guid = Guid.NewGuid();
            log.InfoFormat("A good format: {0:X4}, A bad format: {1:lol}, Another good format: {2}", (short)-23805, guid, true);

            signal.Wait(TimeSpan.FromMilliseconds(100));

            var logMessage = _testAppender.LoggedMessages.Single();
            Check.That(logMessage).Equals("An error occured during formatting: \"A good format: {0:X4}, A bad format: {1:lol}, Another good format: {2}\", -23805, " + guid + ", True");
        }

        [Test]
        public unsafe void should_not_throw_if_formatting_fails_when_appending_formatted_arguments()
        {
            LogManager.RegisterEnum<DayOfWeek>();
            var log = LogManager.GetLogger(typeof(LogManagerTests));
            var signal = _testAppender.SetMessageCountTarget(1);

            var guid = Guid.NewGuid();
            var date = new DateTime(2017, 02, 24, 16, 51, 51);
            var timespan = date.TimeOfDay;
            var asciiString = new[] { (byte)'a', (byte)'b', (byte)'c' };

            fixed (byte* pAsciiString = asciiString)
            {
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
                   .AppendAsciiString(asciiString, asciiString.Length)
                   .AppendAsciiString(pAsciiString, asciiString.Length)
                   .AppendEnum(DayOfWeek.Friday)
                   .Log();
            }

            signal.Wait(TimeSpan.FromMilliseconds(100));

            var logMessage = _testAppender.LoggedMessages.Single();
            Check.That(logMessage).Equals("An error occured during formatting: \"Hello\", False, 1, 'a', 2, 3, 4, 5, 6, 7, " + guid + ", 2017-02-24 16:51:51.000, 16:51:51.000, \"abc\", \"abc\", Friday");
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
            signal.Wait(TimeSpan.FromMilliseconds(500));

            Wait.Until(() => _testAppender.FlushCount == 1, TimeSpan.FromSeconds(1));

            log.Info("Foo");
            Wait.Until(() => _testAppender.FlushCount == 2, TimeSpan.FromSeconds(1));
        }

        [Test]
        public void should_register_all_assembly_enums()
        {
            Check.That(EnumCache.IsRegistered(typeof(ConsoleColor))).IsFalse();
            LogManager.RegisterAllEnumsFrom(typeof(ConsoleColor).Assembly);
            Check.That(EnumCache.IsRegistered(typeof(ConsoleColor))).IsTrue();
        }
    }
}
