using System;
using System.IO;
using System.Text;
using System.Text.Formatting;
using ZeroLog.Utils;

namespace ZeroLog.Appenders
{
    public abstract class AppenderBase<T> : IAppender<T>
    {
        private readonly string[] _formatSpecifiers = { "%date", "%time", "%thread", "%level", "%logger" };
        private readonly StringBuffer _stringBuffer;
        private readonly byte[] _tempBytes;
        private Encoding _encoding;
        private byte[] _newlineBytes = ArrayUtil.Empty<byte>();
        private string _prefixFormat;

        protected AppenderBase()
        {
            _stringBuffer = new StringBuffer(256);
            _tempBytes = new byte[512];
        }

        protected void Configure(string prefixPattern)
        {
            _prefixFormat = BuildPrefixFormat(prefixPattern);
        }

        private string BuildPrefixFormat(string pattern)
        {
            var prefixFormat = pattern.Replace("%date", "{0:yyyy-MM-dd}");
            for (var i = 1; i < _formatSpecifiers.Length; i++)
            {
                prefixFormat = prefixFormat.Replace(_formatSpecifiers[i], $"{{{i}}}");
            }

            return prefixFormat;
        }

        protected int WriteEventToStream(Stream stream, ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength)
        {
            var bytesWritten = 0;
            bytesWritten += WritePrefix(stream, logEventHeader);
            bytesWritten += WriteLine(stream, messageBytes, messageLength);
            return bytesWritten;
        }

        protected unsafe int WritePrefix(Stream stream, ILogEventHeader logEventHeader)
        {
            _stringBuffer.Clear();
            _stringBuffer.AppendFormat(_prefixFormat,
                                       logEventHeader.Timestamp.Date,
                                       logEventHeader.Timestamp.TimeOfDay,
                                       logEventHeader.ThreadId,
                                       LevelStringCache.GetLevelString(logEventHeader.Level),
                                       logEventHeader.Name);

            int bytesWritten;
            fixed (byte* buf = &_tempBytes[0])
                bytesWritten = _stringBuffer.CopyTo(buf, _tempBytes.Length, 0, _stringBuffer.Count, _encoding);

            stream.Write(_tempBytes, 0, bytesWritten);
            return bytesWritten;
        }

        protected int WriteLine(Stream stream, byte[] messageBytes, int messageLength)
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
