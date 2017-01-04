using System;
using System.IO;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog.Appenders
{
    public abstract class AppenderBase : IAppender
    {
        private readonly StringBuffer _stringBuffer;
        private readonly byte[] _tempBytes;
        private Encoding _encoding;
        protected byte[] NewlineBytes;

        protected AppenderBase()
        {
            _stringBuffer = new StringBuffer(256);
            _tempBytes = new byte[512];
        }

        protected unsafe void WritePrefix(Stream stream, LogEvent logEvent)
        {
            _stringBuffer.Clear();
            _stringBuffer.Append("%date{HH:mm:ss.fff}"); // TODO: implement date formatting in StringFormatter
            _stringBuffer.Append(" - ");
            _stringBuffer.Append(logEvent.Level.ToString()); // TODO: Use enum cache
            _stringBuffer.Append(" - ");
            _stringBuffer.Append(logEvent.Name);
            _stringBuffer.Append(" || ");

            int bytesWritten;
            fixed (byte* buf = _tempBytes)
                bytesWritten = _stringBuffer.CopyTo(buf, 0, _stringBuffer.Count, _encoding);

            stream.Write(_tempBytes, 0, bytesWritten);
        }

        public void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
            NewlineBytes = encoding.GetBytes(Environment.NewLine);
        }

        public abstract void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength);
    }
}