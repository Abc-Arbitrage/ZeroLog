using System;
using System.IO;
using System.Text;
using System.Threading;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests.Appenders
{
    [TestFixture]
    public class DateAndSizeRollingFileAppenderTests
    {
        private DateAndSizeRollingFileAppender _appender;

        [SetUp]
        public void SetUp()
        {
            _appender = new DateAndSizeRollingFileAppender("TestLog", prefixPattern: "%date - %time - %thread - %level - %logger || ");
            _appender.SetEncoding(Encoding.Default);
        }

        [TearDown]
        public void Teardown()
        {
            _appender.Dispose();
        }

        [Test, RequiresThread]
        public void should_log_to_file()
        {
            var bytes = new byte[256];
            var message = "Test log message";
            var byteLength = Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);

            var logEventHeader = new LogEventHeader
            {
                Level = Level.Info,
                Name = "TestLog",
                Thread = Thread.CurrentThread,
                Timestamp = DateTime.UtcNow,
            };

            _appender.WriteEvent(logEventHeader, bytes, byteLength);
            _appender.Flush();

            var written = GetLastLine();

            Check.That(written).IsEqualTo($"{logEventHeader.Timestamp.Date:yyyy-MM-dd} - {logEventHeader.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {Thread.CurrentThread.ManagedThreadId} - INFO - TestLog || " + message);
        }

        private string GetLastLine()
        {
            var reader = new StreamReader(File.Open(_appender.CurrentFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            string written = null;

            while (!reader.EndOfStream)
            {
                written = reader.ReadLine();
            }

            return written;
        }
    }
}
