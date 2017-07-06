using System.Collections.Generic;
using System.Text;
using System.Threading;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    public class TestAppender : IAppender
    {
        private readonly bool _captureLoggedMessages;
        private int _messageCount;
        private ManualResetEventSlim _signal;
        private int _messageCountTarget;

        public List<string> LoggedMessages { get; } = new List<string>();

        public TestAppender(bool captureLoggedMessages = true)
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

        public void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            if (_captureLoggedMessages)
                LoggedMessages.Add(Encoding.ASCII.GetString(messageBytes, 0, messageLength));

            if (++_messageCount == _messageCountTarget)
                _signal.Set();
        }

        public void SetEncoding(Encoding encoding)
        {
        }

        public void Close()
        {
        }
    }
}
