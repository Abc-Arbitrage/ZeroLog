using NUnit.Framework;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class UninitializedLogManagerTests
    {
        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_log_without_initialize()
        {
            LogManager.GetLogger("Test").Info($"Test");
        }
    }
}
