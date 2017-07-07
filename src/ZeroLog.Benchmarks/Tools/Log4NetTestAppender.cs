using System.Collections.Generic;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace ZeroLog.Benchmarks
{
    internal class Log4NetTestAppender : AppenderSkeleton
    {
        private readonly bool _captureLoggedMessages;
        private int _messageCount;
        private ManualResetEventSlim _signal;
        private int _messageCountTarget;

        public List<string> LoggedMessages { get; } = new List<string>();

        public Log4NetTestAppender(bool captureLoggedMessages)
        {
            _captureLoggedMessages = captureLoggedMessages;
        }

        public ManualResetEventSlim SetMessageCountTarget(int expectedMessageCount)
        {
            _signal = new ManualResetEventSlim(false);
            _messageCount = 0;
            _messageCountTarget = expectedMessageCount;
            return _signal;
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            var formatted = loggingEvent.RenderedMessage;

            if (_captureLoggedMessages)
                LoggedMessages.Add(loggingEvent.ToString());

            if (++_messageCount == _messageCountTarget)
                _signal.Set();
        }
    }
}