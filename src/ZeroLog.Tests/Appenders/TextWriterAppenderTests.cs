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
public class TextWriterAppenderTests
{
    private static LoggedMessage GetFormattedMessage(string message, out LogMessage logMessage)
    {
        logMessage = new LogMessage(message);
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        return formattedMessage;
    }

    [Test, RequiresThread]
    public void should_append()
    {
        var message = GetFormattedMessage("Test log message", out var logMessage);

        var appender = new TextWriterAppender
        {
            TextWriter = new StringWriter(),
            Formatter = new DefaultFormatter { PrefixPattern = "%date - %time - %thread - %level - %logger || " }
        };

        appender.WriteMessage(message);
        appender.WriteMessage(message);
        appender.Flush();

        var logLine = $"{logMessage.Timestamp.Date:yyyy-MM-dd} - {logMessage.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {Thread.CurrentThread.ManagedThreadId} - INFO - TestLog || {message}{Environment.NewLine}";

        appender.TextWriter.ToString().ShouldEqual(logLine + logLine);
    }

    [Test]
    public void should_change_writer_on_the_fly()
    {
        var before = new StringWriter();
        var after = new StringWriter();

        var appender = new TextWriterAppender(before);
        appender.WriteMessage(GetFormattedMessage("Foo", out _));
        appender.Flush();

        appender.TextWriter = after;
        appender.WriteMessage(GetFormattedMessage("Bar", out _));
        appender.Flush();

        before.ToString().ShouldContain("Foo");
        before.ToString().ShouldNotContain("Bar");

        after.ToString().ShouldContain("Bar");
        after.ToString().ShouldNotContain("Foo");
    }
}
