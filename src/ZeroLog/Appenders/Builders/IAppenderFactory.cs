using ZeroLog.Config;

namespace ZeroLog.Appenders.Builders
{
    public interface IAppenderFactory
    {
        IAppender BuildAppender(AppenderDefinition definition);
    }
}