using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

partial class LogMessageTests
{
    [TestFixture]
    public class MiscTests : LogMessageTests
    {
        [Test]
        public void should_write_empty_message()
            => LogMessage.Empty.ToString().ShouldBeEmpty();

        [Test]
        public void should_write_constant_message()
            => new LogMessage("foobar").ToString().ShouldEqual("foobar");
    }
}
