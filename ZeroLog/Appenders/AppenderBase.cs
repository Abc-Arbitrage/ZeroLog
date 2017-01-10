using System;
using System.IO;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog.Appenders
{
    public abstract class AppenderBase : IAppender
    {
        private readonly string[] _formatSpecifiers = { "%date", "%time", "%thread", "%level", "%logger" };
        private readonly StringBuffer _stringBuffer;
        private readonly byte[] _tempBytes;
        private Encoding _encoding;
        protected byte[] NewlineBytes;
        private readonly string _prefixFormat;

        protected AppenderBase(string prefixPattern)
        {
            _stringBuffer = new StringBuffer(256);
            _tempBytes = new byte[512];
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

        protected unsafe void WritePrefix(Stream stream, LogEvent logEvent)
        {
            _stringBuffer.Clear();
            _stringBuffer.AppendFormat(_prefixFormat,
                                       logEvent.Timestamp.Date,
                                       logEvent.Timestamp.TimeOfDay,
                                       logEvent.ThreadId,
                                       LevelStringCache.GetLevelString(logEvent.Level),
                                       logEvent.Name);

            int bytesWritten;
            fixed (byte* buf = _tempBytes)
                bytesWritten = _stringBuffer.CopyTo(buf, _tempBytes.Length, 0, _stringBuffer.Count, _encoding);

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
