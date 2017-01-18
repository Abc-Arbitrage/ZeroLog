using System.Collections.Generic;
using NFluent;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class LogManagerTests
    {
        [SetUp]
        public void SetUpFixture()
        {
            LogManager.Initialize(new List<IAppender>());
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_create_log()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            Check.That(log).IsNotNull();
        }
        
        [Test, ExpectedException]
        public void should_prevent_initialising_already_initialised_log_manager()
        {
            LogManager.Initialize(new IAppender[0]);
        }
    }
}