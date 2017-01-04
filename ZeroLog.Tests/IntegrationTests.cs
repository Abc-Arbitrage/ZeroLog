using System;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        [Test]
        public void should_test_console()
        {
            LogManager.Initialize(new[] { new ConsoleAppender() });
            Thread.Sleep(1);
            LogManager.GetLogger(typeof(IntegrationTests)).Info().Append("Hello").Log();
            LogManager.Shutdown();
        }
    }
}
