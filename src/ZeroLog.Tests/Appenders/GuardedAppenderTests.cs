using System;
using Moq;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Appenders
{
    [TestFixture]
    public class GuardedAppenderTests
    {
        private Mock<IAppender> _appenderMock;
        private GuardedAppender _guardedAppender;

        [SetUp]
        public void SetUp()
        {
            _appenderMock = new Mock<IAppender>();
            _guardedAppender = new GuardedAppender(_appenderMock.Object, TimeSpan.FromSeconds(15));
        }

        [Test]
        public void should_append()
        {
            var logMessage = new LogMessage("Test");
            var message = new byte[4];

            _guardedAppender.WriteMessage(logMessage, message, message.Length);

            _appenderMock.Verify(x => x.WriteMessage(logMessage, message, message.Length), Times.Once);
        }

        [Test]
        public void should_not_throw_if_inner_appender_throws()
        {
            var logMessage = new LogMessage("Test");
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteMessage(logMessage, message, message.Length)).Throws<Exception>();

            Assert.DoesNotThrow(() => _guardedAppender.WriteMessage(logMessage, message, message.Length));
        }

        [Test]
        public void should_disable_appender_if_it_throws()
        {
            var logMessage = new LogMessage("Test");
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteMessage(logMessage, message, message.Length)).Throws<Exception>();

            _guardedAppender.WriteMessage(logMessage, message, message.Length);
            _guardedAppender.WriteMessage(logMessage, message, message.Length);

            _appenderMock.Verify(x => x.WriteMessage(logMessage, message, message.Length), Times.Once);
        }

        [Test]
        public void should_reenable_appender_after_quarantine_delay()
        {
            var logMessage = new LogMessage("Test");
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteMessage(logMessage, message, message.Length)).Throws<Exception>();

            SystemDateTime.PauseTime();
            _guardedAppender.WriteMessage(logMessage, message, message.Length);
            _appenderMock.Verify(x => x.WriteMessage(logMessage, message, message.Length), Times.Once);

            SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(2));
            _guardedAppender.WriteMessage(logMessage, message, message.Length);
            _appenderMock.Verify(x => x.WriteMessage(logMessage, message, message.Length), Times.Once);

            SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(20));
            _guardedAppender.WriteMessage(logMessage, message, message.Length);
            _appenderMock.Verify(x => x.WriteMessage(logMessage, message, message.Length), Times.Exactly(2));
        }
    }
}
