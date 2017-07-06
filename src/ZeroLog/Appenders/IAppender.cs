using System.Text;

namespace ZeroLog.Appenders
{
    public interface IAppender
    {
        string Name { get; set; }
        void WriteEvent(ILogEventHeader logEventHeader, byte[] messageBytes, int messageLength);
        void SetEncoding(Encoding encoding);
        void Close();
    }
}
