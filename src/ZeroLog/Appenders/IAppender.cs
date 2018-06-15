using System;
using System.Text;

namespace ZeroLog.Appenders
{
    public interface IAppender : IDisposable
    {
        string Name { get; set; }
        void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength);
        void SetEncoding(Encoding encoding);
        void Flush();
    }
}
