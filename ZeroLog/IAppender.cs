using System.IO;

namespace ZeroLog
{
    public interface IAppender
    {
        Stream GetStream();
    }
}