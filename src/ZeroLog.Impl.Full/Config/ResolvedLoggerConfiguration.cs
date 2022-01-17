using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog.Config;

internal sealed class ResolvedLoggerConfiguration
{
    public Level Level { get; }
    public Appender[][] AppendersByLogLevel { get; }
    public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; private init; } = LogMessagePoolExhaustionStrategy.Default;

    private ResolvedLoggerConfiguration(IEnumerable<IEnumerable<Appender>> appendersByLogLevel)
    {
        AppendersByLogLevel = appendersByLogLevel.Select(appenders => appenders.ToArray()).ToArray();
        AppendersByLogLevel[(int)Level.None] = Array.Empty<Appender>();

        Level = Level.None;

        for (var level = 0; level < AppendersByLogLevel.Length; ++level)
        {
            if (AppendersByLogLevel[level].Length == 0)
                continue;

            Level = (Level)level;
            break;
        }
    }

    public static ResolvedLoggerConfiguration Resolve(string loggerName, ZeroLogConfiguration configuration)
    {
        var appendersByLogLevel = new HashSet<Appender>[(int)Level.None + 1];

        for (var i = 0; i < appendersByLogLevel.Length; i++)
            appendersByLogLevel[i] = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        var logMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Default;

        foreach (var loggerConfig in GetOrderedLoggerConfigurations())
            ApplyLoggerConfig(loggerConfig);

        return new ResolvedLoggerConfiguration(appendersByLogLevel)
        {
            LogMessagePoolExhaustionStrategy = logMessagePoolExhaustionStrategy
        };

        IEnumerable<LoggerConfiguration> GetOrderedLoggerConfigurations()
        {
            var loggerNameWithPeriod = $"{loggerName}.";

            return configuration.Loggers
                                .Where(i => loggerNameWithPeriod.StartsWith(i.NameWithPeriod, StringComparison.Ordinal))
                                .OrderBy(i => i.Name.Length)
                                .Prepend(configuration.RootLogger);
        }

        void ApplyLoggerConfig(LoggerConfiguration loggerConfig)
        {
            if (!loggerConfig.IncludeParentAppenders)
            {
                foreach (var appenders in appendersByLogLevel)
                    appenders.Clear();
            }

            foreach (var appenderRef in loggerConfig.Appenders)
            {
                for (var level = (int)appenderRef.Level; level <= (int)Level.None; ++level)
                    appendersByLogLevel[level].Add(appenderRef.Appender);

                for (var level = 0; level <= (int)loggerConfig.Level; ++level)
                    appendersByLogLevel[level].Clear();
            }

            if (loggerConfig.LogMessagePoolExhaustionStrategy != null)
                logMessagePoolExhaustionStrategy = loggerConfig.LogMessagePoolExhaustionStrategy.GetValueOrDefault();
        }
    }

    internal static ResolvedLoggerConfiguration SingleAppender(Level level, Appender? appender = null)
    {
        // For unit tests

        var appenderArray = new[] { appender ?? new NoopAppender() };

        var appendersByLogLevel = Enumerable.Range(0, (int)Level.None + 1)
                                            .Select(i => i >= (int)level ? appenderArray : Array.Empty<Appender>());

        return new ResolvedLoggerConfiguration(appendersByLogLevel);
    }
}
