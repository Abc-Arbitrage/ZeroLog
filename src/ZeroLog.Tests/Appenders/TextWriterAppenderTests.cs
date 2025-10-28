using System;
using System.IO;
using System.Text;
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

        return LoggedMessage.CreateTestMessage(logMessage);
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

        var thread = Thread.CurrentThread.Name ?? Thread.CurrentThread.ManagedThreadId.ToString();
        var logLine = $"{logMessage.Timestamp.Date:yyyy-MM-dd} - {logMessage.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {thread} - INFO - TestLog || {message}{Environment.NewLine}";

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

    [Test]
    public void should_detect_override_of_span_Write()
    {
        TextWriterAppender.OverridesSpanWrite(typeof(WriterWithoutSpanWrite)).ShouldBeFalse();
        TextWriterAppender.OverridesSpanWrite(typeof(WriterWithSpanWrite)).ShouldBeTrue();
    }

    [Test]
    public void should_call_standard_Write_overload()
    {
        var writer = new WriterWithoutSpanWrite();
        var appender = new TextWriterAppender(writer)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" }
        };

        var loggedMessage = LoggedMessage.CreateTestMessage(new LogMessage("Hello"));
        appender.WriteMessage(loggedMessage);

        writer.StandardWriteCalled.ShouldBeTrue();
        writer.StringBuilder.ToString().ShouldEqual($"Hello{Environment.NewLine}");
    }

    [Test]
    public void should_call_span_Write_overload()
    {
        var writer = new WriterWithSpanWrite();
        var appender = new TextWriterAppender(writer)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" }
        };

        var loggedMessage = LoggedMessage.CreateTestMessage(new LogMessage("Hello"));
        appender.WriteMessage(loggedMessage);

        writer.SpanWriteCalled.ShouldBeTrue();
        writer.StandardWriteCalled.ShouldBeFalse();
        writer.StringBuilder.ToString().ShouldEqual($"Hello{Environment.NewLine}");
    }

    [Test]
    public void should_not_allocate([Values] bool span)
    {
        var appender = new TextWriterAppender
        {
            TextWriter = span
                ? new WriterWithSpanWrite { StringBuilder = null }
                : new WriterWithoutSpanWrite { StringBuilder = null }
        };

        var loggedMessage = new LoggedMessage(128, ZeroLogConfiguration.Default);
        loggedMessage.SetMessage(new LogMessage("Hello"));

        GcTester.ShouldNotAllocate(
            () => appender.WriteMessage(loggedMessage)
        );
    }

    private class TextWriterBase : TextWriter
    {
        public override Encoding Encoding => Encoding.Default;

        public StringBuilder StringBuilder { get; init; } = new();

        public bool StandardWriteCalled { get; private set; }

        public override void Write(char[] buffer, int index, int count)
        {
            StandardWriteCalled = true;
            StringBuilder?.Append(buffer, index, count);
        }
    }

    private class WriterWithoutSpanWrite : TextWriterBase;

    private class WriterWithSpanWrite : TextWriterBase
    {
        public bool SpanWriteCalled { get; private set; }

        public override void Write(ReadOnlySpan<char> buffer)
        {
            SpanWriteCalled = true;
            StringBuilder?.Append(buffer);
        }
    }
}
