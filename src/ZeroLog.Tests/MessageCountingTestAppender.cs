using System.Text;
using System.Threading;

namespace ZeroLog.Tests
{
    public class MessageCountingTestAppender : IAppender
    {
        private int _messageCount;
        private ManualResetEventSlim _signal;
        private int _messageCountTarget;

        public ManualResetEventSlim SetMessageCountTarget(int expectedMessageCount)
        {
            _signal = new ManualResetEventSlim(false);
            _messageCount = 0;
            _messageCountTarget = expectedMessageCount;
            return _signal;
        }

        public void WriteEvent(ILogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            if(++_messageCount == _messageCountTarget)
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