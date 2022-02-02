using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
public class StreamAppenderTests
{
    private static FormattedLogMessage GetFormattedMessage(string message, out LogMessage logMessage)
    {
        logMessage = new LogMessage(message);
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        return formattedMessage;
    }

    [Test, RequiresThread]
    public void should_append()
    {
        var message = GetFormattedMessage("Test log message", out var logMessage);

        var appender = new MemoryAppender("%date - %time - %thread - %level - %logger || ");

        appender.WriteMessage(message);
        appender.WriteMessage(message);
        appender.Flush();

        var logLine = $"{logMessage.Timestamp.Date:yyyy-MM-dd} - {logMessage.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {Thread.CurrentThread.ManagedThreadId} - INFO - TestLog || {message}{Environment.NewLine}";

        appender.ToString().ShouldEqual(logLine + logLine);
    }

    [Test]
    public void should_append_with_empty_prefix()
    {
        var message = GetFormattedMessage("Test log message", out _);

        var appender = new MemoryAppender();

        appender.WriteMessage(message);
        appender.WriteMessage(message);
        appender.Flush();

        var logLine = $"{message}{Environment.NewLine}";

        appender.ToString().ShouldEqual(logLine + logLine);
    }

    [Test]
    public void should_append_exceptions()
    {
        var logMessage = new LogMessage("Test log message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);
        logMessage.Exception = new InvalidOperationException("Simulated exception");

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        var appender = new MemoryAppender();

        appender.WriteMessage(formattedMessage);
        appender.Flush();

        appender.ToString().ShouldEqual($"Test log message{Environment.NewLine}{logMessage.Exception}{Environment.NewLine}");
    }

    private sealed class MemoryAppender : StreamAppender
    {
        public MemoryAppender(string prefixPattern = "")
        {
            PrefixPattern = prefixPattern;
            Stream = new MemoryStream();
        }

        public override string ToString()
            => Encoding.GetString(((MemoryStream)Stream!).GetBuffer(), 0, (int)Stream!.Length);
    }
}
