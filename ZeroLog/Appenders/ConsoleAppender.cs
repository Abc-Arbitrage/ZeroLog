using System;
using System.IO;
using System.Text;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : AppenderBase
    {
        private readonly Stream _output;

        public ConsoleAppender()
        {
            _output = Console.OpenStandardOutput();
        }

        public override void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            WritePrefix(_output, logEvent);

            NewlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += NewlineBytes.Length;

            _output.Write(messageBytes, 0, messageLength);
        }
    }
}