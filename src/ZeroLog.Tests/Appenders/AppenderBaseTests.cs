using System;
using System.IO;
using System.Text;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests.Appenders
{
    [TestFixture]
    public class AppenderBaseTests
    {
        [Test]
        public void should_append()
        {
            var bytes = new byte[256];
            const string message = "Test log message";
            var byteLength = Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);

            var logEventHeader = new LogEventHeader
            {
                Level = Level.Info,
                Name = "TestLog",
                ThreadId = 42,
                Timestamp = DateTime.UtcNow,
            };

            var appender = new MemoryAppender("%date - %time - %thread - %level - %logger || ");

            appender.WriteEvent(logEventHeader, bytes, byteLength);
            appender.WriteEvent(logEventHeader, bytes, byteLength);
            appender.Flush();

            var logLine = $"{logEventHeader.Timestamp.Date:yyyy-MM-dd} - {logEventHeader.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fff} - {logEventHeader.ThreadId} - INFO - TestLog || {message}{Environment.NewLine}";

            Check.That(appender.ToString()).IsEqualTo(logLine + logLine);
        }

        [Test]
        public void should_append_with_empty_prefix()
        {
            var bytes = new byte[256];
            const string message = "Test log message";
            var byteLength = Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);

            var logEventHeader = new LogEventHeader();
            var appender = new MemoryAppender("");

            appender.WriteEvent(logEventHeader, bytes, byteLength);
            appender.WriteEvent(logEventHeader, bytes, byteLength);
            appender.Flush();

            var logLine = $"{message}{Environment.NewLine}";

            Check.That(appender.ToString()).IsEqualTo(logLine + logLine);
        }

        [Test]
        public void should_append_newline_even_when_there_are_not_enough_bytes_left_in_buffer()
        {
            var bytes = new byte[4];
            const string message = "Fooo";

            var logEventHeader = new LogEventHeader();
            var appender = new MemoryAppender("");

            Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);
            appender.WriteEvent(logEventHeader, bytes, 2);

            Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);
            appender.WriteEvent(logEventHeader, bytes, 3);

            Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);
            appender.WriteEvent(logEventHeader, bytes, 4);

            appender.Flush();

            Check.That(appender.ToString()).IsEqualTo($"Fo{Environment.NewLine}Foo{Environment.NewLine}Fooo{Environment.NewLine}");
        }

        private sealed class MemoryAppender : AppenderBase<DefaultAppenderConfig>
        {
            private static readonly Encoding _encoding = Encoding.UTF8;
            private readonly MemoryStream _stream = new MemoryStream();

            public MemoryAppender(string prefixPattern)
            {
                Configure(new DefaultAppenderConfig { PrefixPattern = prefixPattern });
                SetEncoding(_encoding);
            }

            public override void Configure(DefaultAppenderConfig parameters)
                => Configure(parameters.PrefixPattern);

            public override void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
                => WriteEventToStream(_stream, logEventHeader, messageBytes, messageLength);

            public override string ToString()
                => _encoding.GetString(_stream.GetBuffer(), 0, (int)_stream.Length);
        }
    }
}
