namespace ZeroLog.Appenders
{
    public class NoopAppender : AppenderBase<object>
    {
        public override void Configure(object parameters)
        {
        }

        public override void WriteMessage(LogMessage message, byte[] messageBytes, int messageLength)
        {
        }
    }
}
