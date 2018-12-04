using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Config
{
    public static class BasicConfigurator
    {
        public static ILogManager Configure(IEnumerable<IAppender> appenders, [CanBeNull] ZeroLogInitializationConfig config = null, Level level = Level.Finest, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.Default)
        {
            var dummyResolver = new BasicResolver(appenders, level, logEventPoolExhaustionStrategy);
            return LogManager.Initialize(dummyResolver, config);
        }

        [Obsolete("Use the overload with the " + nameof(ZeroLogInitializationConfig) + " parameter")]
        public static ILogManager Configure(IEnumerable<IAppender> appenders, int logEventQueueSize, int logEventBufferSize = 128, Level level = Level.Finest, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.Default)
        {
            return Configure(
                appenders,
                new ZeroLogInitializationConfig
                {
                    LogEventQueueSize = logEventQueueSize,
                    LogEventBufferSize = logEventBufferSize
                },
                level,
                logEventPoolExhaustionStrategy
            );
        }
    }
}
