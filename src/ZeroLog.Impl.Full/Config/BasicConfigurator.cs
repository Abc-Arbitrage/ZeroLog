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
        public static IDisposable Configure(ZeroLogBasicConfiguration config)
        {
            config ??= new ZeroLogBasicConfiguration();
            var dummyResolver = new BasicResolver(config.Appenders, config.Level, config.LogMessagePoolExhaustionStrategy);
            return LogManager.Initialize(dummyResolver, config.ToInitializationConfig());
        }

        public static IDisposable Configure(IEnumerable<Appender> appenders)
            => Configure(appenders, new ZeroLogInitializationConfig());

        public static IDisposable Configure(IEnumerable<Appender> appenders, ZeroLogInitializationConfig initializationConfig)
        {
            var config = new ZeroLogBasicConfiguration
            {
                Appenders = appenders.ToList()
            };

            if (initializationConfig != null)
                config.ApplyInitializationConfig(initializationConfig);

            return Configure(config);
        }

        [Obsolete("Use the overload with the " + nameof(ZeroLogBasicConfiguration) + " parameter")]
        [SuppressMessage("ReSharper", "MethodOverloadWithOptionalParameter")]
        public static IDisposable Configure(IEnumerable<Appender> appenders, int logMessagePoolSize = 1024, int logMessageBufferSize = 128, Level level = Level.Trace, LogMessagePoolExhaustionStrategy logMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Default)
        {
            return Configure(new ZeroLogBasicConfiguration
            {
                Appenders = appenders.ToList(),
                Level = level,
                LogMessagePoolSize = logMessagePoolSize,
                LogMessageBufferSize = logMessageBufferSize,
                LogMessagePoolExhaustionStrategy = logMessagePoolExhaustionStrategy
            });
        }
    }
}
