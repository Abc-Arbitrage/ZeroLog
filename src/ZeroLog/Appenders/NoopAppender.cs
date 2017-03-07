namespace ZeroLog.Appenders
{
    public class NoopAppender : AppenderBase
    {
        public NoopAppender()
            : base("")
        {
        }

        public override void WriteEvent(ILogEvent logEvent, byte[] messageBytes, int messageLength)
        {
        }

        public override void Close()
        {
        }
    }
}
