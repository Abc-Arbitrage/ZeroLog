using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests.Appenders
{
    [TestFixture]
    public class PrefixWriterTests
    {
        [Test]
        [TestCase("", "")]
        [TestCase("foo", "foo")]
        [TestCase("%date", "2020-01-02")]
        [TestCase("%time", "03:04:05.0060000")]
        [TestCase("%level", "INFO")]
        [TestCase("%logger", "TestLog")]
        [TestCase("foo %level bar %logger baz", "foo INFO bar TestLog baz")]
        [TestCase("%level %level", "INFO INFO")]
        [TestCase("%LEVEL", "INFO")]
        [TestCase("%{level}", "INFO")]
        [TestCase("%{ level  }", "INFO")]
        [TestCase("foo%{level}Bar", "fooINFOBar")]
        [TestCase("foo%{level}", "fooINFO")]
        [TestCase("foo%level", "fooINFO")]
        [TestCase("%{level}Bar", "INFOBar")]
        [TestCase("%threads", "%threads")]
        [TestCase("%{level}%{logger}", "INFOTestLog")]
        [TestCase("%FOO", "%FOO")]
        [TestCase("%{foo}", "%{foo}")]
        [TestCase("%foo%bar", "%foo%bar")]
        [TestCase("%foo%bar%level%baz", "%foo%barINFO%baz")]
        [TestCase("<%foo>%bar|", "<%foo>%bar|")]
        public void should_write_prefix(string pattern, string expectedResult)
        {
            var prefixWriter = new PrefixWriter(pattern);

            var logEventHeader = new LogEventHeader
            {
                Level = Level.Info,
                Name = "TestLog",
                Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06)
            };

            var result = GetResult(prefixWriter, logEventHeader);
            Check.That(result).IsEqualTo(expectedResult);
        }

        [Test, RequiresThread]
        public void should_write_thread_name()
        {
            Thread.CurrentThread.Name = "Hello";

            var prefixWriter = new PrefixWriter("%thread world!");

            var logEventHeader = new LogEventHeader
            {
                Thread = Thread.CurrentThread
            };

            var result = GetResult(prefixWriter, logEventHeader);
            Check.That(result).IsEqualTo("Hello world!");
        }

        [Test, RequiresThread]
        public void should_write_thread_id()
        {
            var prefixWriter = new PrefixWriter("%thread");

            var logEventHeader = new LogEventHeader
            {
                Thread = Thread.CurrentThread
            };

            var result = GetResult(prefixWriter, logEventHeader);
            Check.That(result).IsEqualTo(Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
        }

        [Test, RequiresThread]
        public void should_write_zero_when_no_thread_provided()
        {
            var prefixWriter = new PrefixWriter("%thread");

            var logEventHeader = new LogEventHeader
            {
                Thread = null
            };

            var result = GetResult(prefixWriter, logEventHeader);
            Check.That(result).IsEqualTo("0");
        }

        private static string GetResult(PrefixWriter prefixWriter, ILogEventHeader logEventHeader)
        {
            using var stream = new MemoryStream();
            var bytesWritten = prefixWriter.WritePrefix(stream, logEventHeader, Encoding.UTF8);
            Check.That(bytesWritten).IsEqualTo((int)stream.Position);

            return Encoding.UTF8.GetString(stream.GetBuffer(), 0, bytesWritten);
        }
    }
}
