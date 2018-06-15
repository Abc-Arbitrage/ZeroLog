namespace ZeroLog.Appenders
{
    public class NoopAppender : AppenderBase<object>
    {
        public override void Configure(object parameters)
        {
        }

        public override void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
        }
    }
}
