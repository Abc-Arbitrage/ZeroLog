using System;
using System.IO;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog.Appenders
{
    public abstract class AppenderBase : IAppender
    {
        protected readonly StringBuffer StringBuffer;
        protected readonly byte[] TempBytes;
        private Encoding _encoding;
        protected byte[] NewlineBytes;

        public AppenderBase( )
        {
            StringBuffer = new StringBuffer(256);
            TempBytes = new byte[512];
        }


        protected unsafe void WritePrefix(Stream stream, LogEvent logEvent)
        {
            StringBuffer.Append("%date{HH:mm:ss.fff}"); // TODO: implement date formatting in StringFormatter
            StringBuffer.Append(" - ");
            StringBuffer.Append(logEvent.Level.ToString()); // TODO: Use enum cache
            StringBuffer.Append(" - ");
            StringBuffer.Append(logEvent.Name);
            StringBuffer.Append(" || ");

            int bytesWritten;
            fixed (byte* buf = TempBytes)
                bytesWritten = StringBuffer.CopyTo(buf, 0, StringBuffer.Count, _encoding);

            stream.Write(TempBytes, 0, bytesWritten);
        }

        public void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
            NewlineBytes = encoding.GetBytes(Environment.NewLine);
        }

        public abstract void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength);
    }
}