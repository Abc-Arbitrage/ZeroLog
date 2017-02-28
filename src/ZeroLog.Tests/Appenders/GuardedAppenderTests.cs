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
            var logEvent = new Mock<ILogEvent>().Object;
            var message = new byte[4];

            _guardedAppender.WriteEvent(logEvent, message, message.Length);

            _appenderMock.Verify(x => x.WriteEvent(logEvent, message, message.Length), Times.Once);
        }

        [Test]
        public void should_not_throw_if_inner_appender_throws()
        {
            var logEvent = new Mock<ILogEvent>().Object;
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteEvent(logEvent, message, message.Length)).Throws<Exception>();

            Assert.DoesNotThrow(() => _guardedAppender.WriteEvent(logEvent, message, message.Length));
        }

        [Test]
        public void should_disable_appender_if_it_throws()
        {
            var logEvent = new Mock<ILogEvent>().Object;
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteEvent(logEvent, message, message.Length)).Throws<Exception>();

            _guardedAppender.WriteEvent(logEvent, message, message.Length);
            _guardedAppender.WriteEvent(logEvent, message, message.Length);

            _appenderMock.Verify(x => x.WriteEvent(logEvent, message, message.Length), Times.Once);
        }

        [Test]
        public void should_reenable_appender_after_quarantine_delay()
        {
            var logEvent = new Mock<ILogEvent>().Object;
            var message = new byte[4];
            _appenderMock.Setup(x => x.WriteEvent(logEvent, message, message.Length)).Throws<Exception>();

            SystemDateTime.PauseTime();
            _guardedAppender.WriteEvent(logEvent, message, message.Length);
            _appenderMock.Verify(x => x.WriteEvent(logEvent, message, message.Length), Times.Once);

            SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(2));
            _guardedAppender.WriteEvent(logEvent, message, message.Length);
            _appenderMock.Verify(x => x.WriteEvent(logEvent, message, message.Length), Times.Once);

            SystemDateTime.AddToPausedTime(TimeSpan.FromSeconds(20));
            _guardedAppender.WriteEvent(logEvent, message, message.Length);
            _appenderMock.Verify(x => x.WriteEvent(logEvent, message, message.Length), Times.Exactly(2));
        }
    }
}