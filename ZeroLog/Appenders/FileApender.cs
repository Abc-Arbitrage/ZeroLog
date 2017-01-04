using System.IO;
using System.Text;

namespace ZeroLog.Appenders
{
    public class FileApender : IAppender
    {
        public void WriteEvent(LogEvent logEvent, byte[] messageBytes, int messageLength)
        {
            throw new System.NotImplementedException();
        }

        public void SetEncoding(Encoding encoding)
        {
            throw new System.NotImplementedException();
        }
    }
}