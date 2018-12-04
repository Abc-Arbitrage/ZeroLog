using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Config
{
    public static class BasicConfigurator
    {
        public static ILogManager Configure(ZeroLogBasicConfig config)
        {
            config = config ?? new ZeroLogBasicConfig();
            var dummyResolver = new BasicResolver(config.Appenders, config.Level, config.LogEventPoolExhaustionStrategy);
            return LogManager.Initialize(dummyResolver, config.ToInitializationConfig());
        }

        public static ILogManager Configure(IEnumerable<IAppender> appenders)
            => Configure(appenders, new ZeroLogInitializationConfig());

        public static ILogManager Configure(IEnumerable<IAppender> appenders, ZeroLogInitializationConfig initializationConfig)
        {
            var config = new ZeroLogBasicConfig
            {
                Appenders = appenders.ToList()
            };

            if (initializationConfig != null)
                config.ApplyInitializationConfig(initializationConfig);

            return Configure(config);
        }

        [Obsolete("Use the overload with the " + nameof(ZeroLogBasicConfig) + " parameter")]
        [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
        public static ILogManager Configure(IEnumerable<IAppender> appenders, int logEventQueueSize = 1024, int logEventBufferSize = 128, Level level = Level.Finest, LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.Default)
        {
            return Configure(new ZeroLogBasicConfig
            {
                Appenders = appenders.ToList(),
                Level = level,
                LogEventQueueSize = logEventQueueSize,
                LogEventBufferSize = logEventBufferSize,
                LogEventPoolExhaustionStrategy = logEventPoolExhaustionStrategy
            });
        }
    }
}
