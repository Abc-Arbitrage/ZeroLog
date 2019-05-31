using System;
using System.Diagnostics.CodeAnalysis;
using Moq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class LogTests
    {
        private TestAppender _appender;
        private Mock<IConfigurationResolver> _configResolver;
        private LogManager _logManager;

        [SetUp]
        public void SetUp()
        {
            _appender = new TestAppender(true);

            _configResolver = new Mock<IConfigurationResolver>();
            _configResolver.Setup(x => x.ResolveLogConfig(It.IsAny<string>()))
                           .Returns(new LogConfig
                           {
                               Level = Level.Debug,
                               Appenders = new IAppender[] { _appender }
                           });

            _logManager = new LogManager(_configResolver.Object, new ZeroLogInitializationConfig { LogEventQueueSize = 16, LogEventBufferSize = 128 });
        }

        [TearDown]
        public void Teardown()
        {
            _logManager.Dispose();
        }

        [TestCase(Level.Finest, true, true, true, true, true)]
        [TestCase(Level.Verbose, true, true, true, true, true)]
        [TestCase(Level.Debug, true, true, true, true, true)]
        [TestCase(Level.Info, false, true, true, true, true)]
        [TestCase(Level.Warn, false, false, true, true, true)]
        [TestCase(Level.Error, false, false, false, true, true)]
        [TestCase(Level.Fatal, false, false, false, false, true)]
        public void should_return_if_log_level_is_enabled(Level logLevel, bool isDebug, bool isInfo, bool isWarn, bool isError, bool isFatal)
        {
            _configResolver.Setup(x => x.ResolveLogConfig(It.IsAny<string>()))
                           .Returns(new LogConfig { Level = logLevel });

            var log = new Log(_logManager, "logger");

            Check.That(log.IsDebugEnabled).Equals(isDebug);
            Check.That(log.IsInfoEnabled).Equals(isInfo);
            Check.That(log.IsWarnEnabled).Equals(isWarn);
            Check.That(log.IsErrorEnabled).Equals(isError);
            Check.That(log.IsFatalEnabled).Equals(isFatal);

            Check.That(log.IsLevelEnabled(logLevel)).IsTrue();
            if (logLevel > Level.Finest)
                Check.That(log.IsLevelEnabled(logLevel - 1)).IsFalse();
            if (logLevel < Level.Fatal)
                Check.That(log.IsLevelEnabled(logLevel + 1)).IsTrue();
        }

        [Test]
        public void should_log_primitive_types_with_simple_api()
        {
            var log = new Log(_logManager, "logger");

            log.InfoFormat("foo {0} bar", 42);
            log.InfoFormat("foo {0} bar", (int?)42);
            log.InfoFormat("foo {0} bar", (int?)null);

            WaitForEmptyQueue();

            Check.That(_appender.LoggedMessages).ContainsExactly(
                "foo 42 bar",
                "foo 42 bar",
                "foo null bar"
            );
        }

        [Test]
        public void should_log_enums_with_simple_api()
        {
            LogManager.RegisterEnum<DayOfWeek>();

            var log = new Log(_logManager, "logger");

            log.InfoFormat("foo {0} bar", DayOfWeek.Friday);
            log.InfoFormat("foo {0} bar", (DayOfWeek?)DayOfWeek.Friday);
            log.InfoFormat("foo {0} bar", (DayOfWeek?)null);

            WaitForEmptyQueue();

            Check.That(_appender.LoggedMessages).ContainsExactly(
                "foo Friday bar",
                "foo Friday bar",
                "foo null bar"
            );
        }

        [Test]
        public void should_log_datetime_with_simple_api()
        {
            var log = new Log(_logManager, "logger");

            var dateTime = new DateTime(2000, 1, 2, 3, 4, 5, 6);

            log.InfoFormat("foo {0} bar", dateTime);
            log.InfoFormat("foo {0:d} bar", dateTime);
            log.InfoFormat("foo {0:yyyy-MM-dd} bar", dateTime);
            log.InfoFormat("foo {0} bar", (DateTime?)dateTime);
            log.InfoFormat("foo {0:d} bar", (DateTime?)dateTime);
            log.InfoFormat("foo {0} bar", (DateTime?)null);
            log.InfoFormat("foo {0:d} bar", (DateTime?)null);

            WaitForEmptyQueue();

            Check.That(_appender.LoggedMessages).ContainsExactly(
                "foo 2000-01-02 03:04:05.006 bar",
                "foo 2000-01-02 bar",
                "foo 2000-01-02 bar",
                "foo 2000-01-02 03:04:05.006 bar",
                "foo 2000-01-02 bar",
                "foo null bar",
                "foo null bar"
            );
        }

        [Test]
        public void should_log_timespan_with_simple_api()
        {
            var log = new Log(_logManager, "logger");

            var timeSpan = new TimeSpan(1, 2, 3, 4, 5);

            log.InfoFormat("foo {0} bar", timeSpan);
            log.InfoFormat("foo {0:c} bar", timeSpan);
            log.InfoFormat("foo {0:g} bar", timeSpan);
            log.InfoFormat("foo {0:G} bar", timeSpan);
            log.InfoFormat("foo {0} bar", (TimeSpan?)timeSpan);
            log.InfoFormat("foo {0:g} bar", (TimeSpan?)timeSpan);
            log.InfoFormat("foo {0} bar", (TimeSpan?)null);
            log.InfoFormat("foo {0:g} bar", (TimeSpan?)null);

            WaitForEmptyQueue();

            Check.That(_appender.LoggedMessages).ContainsExactly(
                "foo 1.02:03:04.0050000 bar",
                "foo 1.02:03:04.0050000 bar",
                "foo 1:2:03:04.005 bar",
                "foo 1:02:03:04.0050000 bar",
                "foo 1.02:03:04.0050000 bar",
                "foo 1:2:03:04.005 bar",
                "foo null bar",
                "foo null bar"
            );
        }

        [Test]
        [SuppressMessage("ReSharper", "FormatStringProblem")]
        public void should_log_unmanaged_types_with_simple_api()
        {
            LogManager.RegisterUnmanaged<LogEventTests.UnmanagedStruct>();
            LogManager.RegisterUnmanaged<LogEventTests.UnmanagedStructWithFormatSupport>();

            var log = new Log(_logManager, "logger");

            var valueNormal = new LogEventTests.UnmanagedStruct
            {
                A = 1,
                B = 2,
                C = 3
            };

            var valueFormattable = new LogEventTests.UnmanagedStructWithFormatSupport
            {
                A = 42
            };

            log.InfoFormat("foo {0} bar", valueNormal);
            log.InfoFormat("foo {0} bar", (LogEventTests.UnmanagedStruct?)valueNormal);
            log.InfoFormat("foo {0} bar", (LogEventTests.UnmanagedStruct?)null);

            log.InfoFormat("foo {0:baz} bar", valueFormattable);
            log.InfoFormat("foo {0:baz} bar", (LogEventTests.UnmanagedStructWithFormatSupport?)valueFormattable);
            log.InfoFormat("foo {0:baz} bar", (LogEventTests.UnmanagedStructWithFormatSupport?)null);

            WaitForEmptyQueue();

            Check.That(_appender.LoggedMessages).ContainsExactly(
                "foo 1-2-3 bar",
                "foo 1-2-3 bar",
                "foo null bar",
                "foo 42[baz] bar",
                "foo 42[baz] bar",
                "foo null bar"
            );
        }

        private void WaitForEmptyQueue()
        {
            var queue = _logManager.GetInternalQueue();
            Wait.Until(() => queue.IsEmpty, TimeSpan.FromSeconds(1));
        }
    }
}
