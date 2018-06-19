using System;
using System.IO;
using System.Text;
using ZeroLog.Utils;

namespace ZeroLog.Appenders
{
    public abstract class AppenderBase<T> : IAppender<T>
    {
        private Encoding _encoding;
        private byte[] _newlineBytes = ArrayUtil.Empty<byte>();
        private PrefixWriter _prefixWriter;

        protected void Configure(string prefixPattern)
        {
            _prefixWriter = new PrefixWriter(prefixPattern);
        }

        protected int WriteEventToStream(Stream stream, ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            var bytesWritten = 0;
            bytesWritten += _prefixWriter.WritePrefix(stream, logEventHeader, _encoding);
            bytesWritten += WriteLine(stream, messageBytes, messageLength);
            return bytesWritten;
        }

        private int WriteLine(Stream stream, byte[] messageBytes, int messageLength)
        {
            var newlineBytes = _newlineBytes;

            if (messageLength + newlineBytes.Length < messageBytes.Length)
            {
                Array.Copy(newlineBytes, 0, messageBytes, messageLength, newlineBytes.Length);
                messageLength += newlineBytes.Length;

                stream.Write(messageBytes, 0, messageLength);
                return messageLength;
            }

            stream.Write(messageBytes, 0, messageLength);
            stream.Write(newlineBytes, 0, newlineBytes.Length);
            return messageLength + newlineBytes.Length;
        }

        public void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
            _newlineBytes = encoding.GetBytes(Environment.NewLine);
        }

        public virtual void Flush()
        {
        }

        public virtual void Dispose()
        {
        }

        public string Name { get; set; }
        public abstract void Configure(T parameters);
        public abstract void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength);
    }
}
