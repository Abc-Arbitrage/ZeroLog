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
    }
}
