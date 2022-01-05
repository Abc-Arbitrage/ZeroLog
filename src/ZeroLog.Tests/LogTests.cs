using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class LogTests
{
    [TestCase(Level.Trace, true, true, true, true, true, true)]
    [TestCase(Level.Debug, false, true, true, true, true, true)]
    [TestCase(Level.Info, false, false, true, true, true, true)]
    [TestCase(Level.Warn, false, false, false, true, true, true)]
    [TestCase(Level.Error, false, false, false, false, true, true)]
    [TestCase(Level.Fatal, false, false, false, false, false, true)]
    public void should_tell_if_log_level_is_enabled(Level logLevel, bool isTrace, bool isDebug, bool isInfo, bool isWarn, bool isError, bool isFatal)
    {
        var log = new Log("logger");
        log.UpdateConfiguration(null, new LogConfig { Level = logLevel });

        log.IsTraceEnabled.ShouldEqual(isTrace);
        log.IsDebugEnabled.ShouldEqual(isDebug);
        log.IsInfoEnabled.ShouldEqual(isInfo);
        log.IsWarnEnabled.ShouldEqual(isWarn);
        log.IsErrorEnabled.ShouldEqual(isError);
        log.IsFatalEnabled.ShouldEqual(isFatal);

        log.IsEnabled(logLevel).ShouldBeTrue();
        log.IsEnabled(logLevel - 1).ShouldBeFalse();
        log.IsEnabled(logLevel + 1).ShouldBeTrue();
    }
}
