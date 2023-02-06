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
public class StreamAppenderTests
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

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        var appender = new MemoryAppender();

        appender.WriteMessage(formattedMessage);
        appender.Flush();

        appender.ToString().ShouldEqual($"Test log message{Environment.NewLine}{logMessage.Exception}{Environment.NewLine}");
    }

    [Test]
    public void should_detect_override_of_span_GetBytes()
    {
        StreamAppender.OverridesSpanGetBytes(typeof(EncodingWithoutSpanGetBytes)).ShouldBeFalse();
        StreamAppender.OverridesSpanGetBytes(typeof(EncodingWithSpanGetBytes)).ShouldBeTrue();
    }

    [Test]
    public void should_call_array_GetBytes_overload()
    {
        var encoding = new EncodingWithoutSpanGetBytes();
        var appender = new MemoryAppender();
        appender.SetEncoding(encoding);

        var loggedMessage = new LoggedMessage(128, ZeroLogConfiguration.Default);
        loggedMessage.SetMessage(new LogMessage("Hello"));
        appender.WriteMessage(loggedMessage);

        encoding.StandardGetBytesCalled.ShouldBeTrue();
        appender.ToString().ShouldEqual($"Hello{Environment.NewLine}");
    }

    [Test]
    public void should_call_span_GetBytes_overload()
    {
        var encoding = new EncodingWithSpanGetBytes();
        var appender = new MemoryAppender();
        appender.SetEncoding(encoding);

        var loggedMessage = new LoggedMessage(128, ZeroLogConfiguration.Default);
        loggedMessage.SetMessage(new LogMessage("Hello"));
        appender.WriteMessage(loggedMessage);

        encoding.SpanGetBytesCalled.ShouldBeTrue();
        encoding.StandardGetBytesCalled.ShouldBeFalse();
        appender.ToString().ShouldEqual($"Hello{Environment.NewLine}");
    }

    [Test]
    public void should_not_allocate([Values] bool span)
    {
        var appender = new AllocationTestsAppender(span);

        var loggedMessage = new LoggedMessage(128, ZeroLogConfiguration.Default);
        loggedMessage.SetMessage(new LogMessage("Hello"));

        GcTester.ShouldNotAllocate(
            () => appender.WriteMessage(loggedMessage)
        );
    }

    private sealed class MemoryAppender : StreamAppender
    {
        public MemoryAppender(string prefixPattern = "")
        {
            Formatter = new DefaultFormatter { PrefixPattern = prefixPattern };
            Stream = new MemoryStream();
        }

        public void SetEncoding(Encoding encoding)
        {
            Encoding = encoding;
        }

        public override string ToString()
            => Encoding.GetString(((MemoryStream)Stream!).GetBuffer(), 0, (int)Stream!.Length);
    }

    private sealed class AllocationTestsAppender : StreamAppender
    {
        public AllocationTestsAppender(bool span)
        {
            Stream = Stream.Null;
            Encoding = span ? new EncodingWithSpanGetBytes() : new EncodingWithoutSpanGetBytes();
            Formatter = new DefaultFormatter { PrefixPattern = "%level " };
        }
    }

    private class EncodingBase : Encoding
    {
        public bool StandardGetBytesCalled { get; private set; }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            StandardGetBytesCalled = true;
            return Default.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
        }

        public override int GetMaxByteCount(int charCount)
            => Default.GetMaxByteCount(charCount);

        public override int GetByteCount(char[] chars, int index, int count)
            => Default.GetByteCount(chars, index, count);

        public override int GetCharCount(byte[] bytes, int index, int count)
            => Default.GetCharCount(bytes, index, count);

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            => Default.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

        public override int GetMaxCharCount(int byteCount)
            => Default.GetMaxCharCount(byteCount);
    }

    private class EncodingWithoutSpanGetBytes : EncodingBase
    {
    }

    private class EncodingWithSpanGetBytes : EncodingBase
    {
        public bool SpanGetBytesCalled { get; private set; }

        public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
        {
            SpanGetBytesCalled = true;
            return Default.GetBytes(chars, bytes);
        }
    }
}
