using NUnit.Framework;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class UninitialisedLogManagerTests
    {
        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_log_without_initialise()
        {
            LogManager.GetLogger("Test").Info("Test");
        }
    }
}