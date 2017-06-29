using System.Collections.Generic;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class LogTests
    {
        [TestCase(Level.Finest, true, true, true, true, true)]
        [TestCase(Level.Verbose, true, true, true, true, true)]
        [TestCase(Level.Debug, true, true, true, true, true)]
        [TestCase(Level.Info, false, true, true, true, true)]
        [TestCase(Level.Warn, false, false, true, true, true)]
        [TestCase(Level.Error, false, false, false, true, true)]
        [TestCase(Level.Fatal, false, false, false, false, true)]
        public void should_return_if_log_level_is_enabled(Level logLevel, bool isDebug, bool isInfo, bool isWarn, bool isError, bool isFatal)
        {
            var configuration = new LogManagerConfiguration
            {
                LogEventQueueSize = 1,
                LogEventBufferSize = 128,
                Level = logLevel,
            };

            var logManager = new LogManager(new List<IAppender>(), configuration);
            var log = new Log(logManager, "logger");

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
