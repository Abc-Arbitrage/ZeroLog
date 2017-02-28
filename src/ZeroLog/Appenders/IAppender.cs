using System.Text;

namespace ZeroLog.Appenders
{
    public interface IAppender
    {
        void WriteEvent(ILogEvent logEvent, byte[] messageBytes, int messageLength);
        void SetEncoding(Encoding encoding);
        void Close();
    }
}
