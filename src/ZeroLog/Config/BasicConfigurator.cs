using System.Collections.Generic;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Config
{
    public static class BasicConfigurator
    {
        public static ILogManager Configure(IEnumerable<IAppender> appenders, int logEventQueueSize = 1024, int logEventBufferSize = 128, Level level = Level.Finest, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.Default)
        {
            var dummyResolver = new BasicResolver(appenders, level, logEventPoolExhaustionStrategy);
            return LogManager.Initialize(dummyResolver, logEventQueueSize, logEventBufferSize);
        }
    }
}