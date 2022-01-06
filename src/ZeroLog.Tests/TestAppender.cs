using System.Collections.Generic;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    public class TestAppender : IAppender
    {
        private readonly bool _captureLoggedMessages;
        private int _messageCount;
        private ManualResetEventSlim _signal;
        private int _messageCountTarget;

        public List<string> LoggedMessages { get; } = new();
        public int FlushCount { get; set; }

        public ManualResetEventSlim WaitOnWriteEvent { get; set; }

        [UsedImplicitly]
        public TestAppender()
        {
        }

        public TestAppender(bool captureLoggedMessages)
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

        public void WriteMessage(FormattedLogMessage message)
        {
            if (_captureLoggedMessages)
                LoggedMessages.Add(message.ToString());

            if (++_messageCount == _messageCountTarget)
                _signal.Set();

            WaitOnWriteEvent?.Wait();
        }

        public void Flush()
        {
            ++FlushCount;
        }

        public void Dispose()
        {
        }
    }
}
