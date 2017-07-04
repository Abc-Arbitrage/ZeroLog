using System.Runtime.InteropServices;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders.Builders;

namespace ZeroLog.Tests.Appenders.Builder
{
    [TestFixture]
    public class ConsoleAppenderBuilderTests
    {
        private ConsoleAppenderBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new ConsoleAppenderBuilder();
        }


        [Test]
        public void should_handle_missing_config()
        {
            Assert.DoesNotThrow(() =>_builder.BuildAppender("SomeName", null));
        }
    }
}
