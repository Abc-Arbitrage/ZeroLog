using System;
using System.IO;
using System.Text;
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
        [TestCase("%thread", "42")]
        [TestCase("%level", "INFO")]
        [TestCase("%logger", "TestLog")]
        [TestCase("foo %level bar %logger baz", "foo INFO bar TestLog baz")]
        [TestCase("%level %level", "INFO INFO")]
        [TestCase("%LEVEL", "INFO")]
        [TestCase("%{level}", "INFO")]
        [TestCase("%{ level  }", "INFO")]
        [TestCase("foo%{thread}bar", "foo42bar")]
        [TestCase("foo%{thread}", "foo42")]
        [TestCase("foo%thread", "foo42")]
        [TestCase("%{thread}bar", "42bar")]
        [TestCase("%threads", "%threads")]
        [TestCase("%{level}%{thread}", "INFO42")]
        [TestCase("%FOO", "%FOO")]
        [TestCase("%{foo}", "%{foo}")]
        [TestCase("%foo%bar", "%foo%bar")]
        [TestCase("%foo%bar%thread%baz", "%foo%bar42%baz")]
        [TestCase("<%foo>%bar|", "<%foo>%bar|")]
        public void should_write_prefix(string pattern, string expectedResult)
        {
            var prefixWriter = new PrefixWriter(pattern);

            var logEventHeader = new LogEventHeader
            {
                Level = Level.Info,
                Name = "TestLog",
                ThreadId = 42,
                Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06)
            };

            using var stream = new MemoryStream();
            var bytesWritten = prefixWriter.WritePrefix(stream, logEventHeader, Encoding.UTF8);
            Check.That(bytesWritten).IsEqualTo((int)stream.Position);

            var result = Encoding.UTF8.GetString(stream.GetBuffer(), 0, bytesWritten);
            Check.That(result).IsEqualTo(expectedResult);
        }
    }
}
