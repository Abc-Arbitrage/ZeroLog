using System;
using System.IO;
using System.Text;

namespace ZeroLog.Appenders
{
    public class ConsoleAppender : IAppender
    {
        private readonly Stream _output;
        private byte[] _newlineBytes;
        private Encoding _encoding;

        public ConsoleAppender()
        {
            _output = Console.OpenStandardOutput();
        }

        public void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            _newlineBytes.CopyTo(messageBytes, messageLength);
            messageLength += _newlineBytes.Length;

            _output.Write(messageBytes, 0, messageLength);
        }

        public void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
            _newlineBytes = encoding.GetBytes(Environment.NewLine);
        }
    }
}