using System.Linq;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

[TestFixture]
public class SanityChecks
{
    [Test]
    public void should_export_expected_types()
    {
        // This test prevents mistakenly adding public types in the future.

        var publicTypes = new[]
        {
            "ZeroLog.Appenders.Appender",
            "ZeroLog.Appenders.ConsoleAppender",
            "ZeroLog.Appenders.DateAndSizeRollingFileAppender",
            "ZeroLog.Appenders.NoopAppender",
            "ZeroLog.Appenders.StreamAppender",
            "ZeroLog.Configuration.AppenderConfiguration",
            "ZeroLog.Configuration.LoggerConfiguration",
            "ZeroLog.Configuration.LogMessagePoolExhaustionStrategy",
            "ZeroLog.Configuration.RootLoggerConfiguration",
            "ZeroLog.Configuration.ZeroLogConfiguration",
            "ZeroLog.Formatting.FormattedLogMessage",
            "ZeroLog.Log",
            "ZeroLog.Log+DebugInterpolatedStringHandler",
            "ZeroLog.Log+ErrorInterpolatedStringHandler",
            "ZeroLog.Log+FatalInterpolatedStringHandler",
            "ZeroLog.Log+InfoInterpolatedStringHandler",
            "ZeroLog.Log+TraceInterpolatedStringHandler",
            "ZeroLog.Log+WarnInterpolatedStringHandler",
            "ZeroLog.LogLevel",
            "ZeroLog.LogManager",
            "ZeroLog.LogMessage",
            "ZeroLog.LogMessage+AppendInterpolatedStringHandler",
            "ZeroLog.UnmanagedFormatterDelegate`1",
        };

        typeof(LogManager).Assembly
                          .ExportedTypes
                          .Select(i => i.FullName)
                          .ShouldBeEquivalentTo(publicTypes);
    }
}
