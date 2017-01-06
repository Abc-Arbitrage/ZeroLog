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
            _appender = new DateAndSizeRollingFileAppender("TestLog");
            _appender.SetEncoding(Encoding.Default);
        }

        [TearDown]
        public void Teardown()
        {
            _appender.Close();
        }

        [Test]
        public void should_log_to_file()
        {
            var bytes = new byte[256];
            var message = "Test log message";
            var byteLength = Encoding.Default.GetBytes(message, 0, message.Length, bytes, 0);
            var bufferSegmentProvider = new BufferSegmentProvider(1024, 1024);
            var logEvent = new LogEvent(Level.Info, bufferSegmentProvider.GetSegment());
            logEvent.Initialize(Level.Info, new Log(null, "TestLog"));
            _appender.WriteEvent(logEvent, bytes, byteLength);
            _appender.Flush();
            
            var written = GetLastLine();

            Check.That(written).IsEqualTo($"{logEvent.Timestamp.TimeOfDay.ToString(@"hh\:mm\:ss\.fff")} - Info - TestLog || " + message);
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
