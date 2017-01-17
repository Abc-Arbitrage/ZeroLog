using System.IO;
using System.Text;

namespace ZeroLog
{
    public interface IAppender
    {
        void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength);
        void SetEncoding(Encoding encoding);
        void Close();
    }
}