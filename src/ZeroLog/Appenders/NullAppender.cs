using System.IO;

namespace ZeroLog.Appenders
{
    public class NullAppender : AppenderBase
    {
        private readonly MemoryStream _output = new MemoryStream(1024);

        public NullAppender(): base("")
        {
        }

        public override void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            /*_output.Seek(0, SeekOrigin.Begin);

            WritePrefix(_output, logEvent);

            NewlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += NewlineBytes.Length;

            _output.Write(messageBytes, 0, messageLength);*/
        }

        public override void Close()
        {
        }
    }
}