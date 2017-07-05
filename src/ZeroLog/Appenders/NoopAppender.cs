namespace ZeroLog.Appenders
{
    public class NoopAppender : AppenderBase<object>
    {
        public override void Configure(object parameters)
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
