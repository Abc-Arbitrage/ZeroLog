using System.IO;
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
            _appender = new DateAndSizeRollingFileAppender("TestLog")
            {
                PrefixPattern = "%date - %time - %thread - %level - %logger || "
            };
        }

        [TearDown]
        public void Teardown()
        {
            _appender.Dispose();
        }

        [Test, RequiresThread]
        public void should_log_to_file()
        {
            var logMessage = new LogMessage("Test log message");
            logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

            var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length);
            formattedMessage.SetMessage(logMessage);

            _appender.WriteMessage(formattedMessage);
            _appender.Flush();

            var written = GetLastLine();

            Check.That(written).IsEqualTo($"{logMessage.Timestamp.Date:yyyy-MM-dd} - {logMessage.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {Thread.CurrentThread.ManagedThreadId} - INFO - TestLog || {logMessage}");
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
