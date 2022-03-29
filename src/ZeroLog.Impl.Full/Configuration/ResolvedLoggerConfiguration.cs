using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ZeroLog.Appenders;

namespace ZeroLog.Configuration;

internal sealed class ResolvedLoggerConfiguration
{
    private const int _levelCount = (int)LogLevel.None + 1;

    public static ResolvedLoggerConfiguration Empty { get; } = new(Enumerable.Repeat(Array.Empty<Appender>(), _levelCount))
    {
        LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage
    };

    private readonly Appender[][] _appendersByLogLevel;

    public LogLevel Level { get; }
    public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; private init; } = LogMessagePoolExhaustionStrategy.Default;

    private ResolvedLoggerConfiguration(IEnumerable<Appender[]> appendersByLogLevel)
    {
        _appendersByLogLevel = appendersByLogLevel.ToArray();
        _appendersByLogLevel[(int)LogLevel.None] = Array.Empty<Appender>();
        Debug.Assert(_appendersByLogLevel.Length == _levelCount);

        Level = LogLevel.None;

        for (var level = 0; level < _appendersByLogLevel.Length; ++level)
        {
            if (_appendersByLogLevel[level].Length == 0)
                continue;

            Level = (LogLevel)level;
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Appender[] GetAppenders(LogLevel level)
        => _appendersByLogLevel[(int)level];

    public static ResolvedLoggerConfiguration Resolve(string loggerName, ZeroLogConfiguration configuration)
    {
        var appendersByLogLevel = new HashSet<Appender>[_levelCount];

        for (var i = 0; i < appendersByLogLevel.Length; i++)
            appendersByLogLevel[i] = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        var effectiveLevel = configuration.RootLogger.Level;
        var effectiveLogMessagePoolExhaustionStrategy = configuration.RootLogger.LogMessagePoolExhaustionStrategy;

        foreach (var loggerConfig in GetOrderedLoggerConfigurations())
            ApplyLoggerConfig(loggerConfig);

        for (var level = 0; level < (int)effectiveLevel; ++level)
            appendersByLogLevel[level].Clear();

        return new ResolvedLoggerConfiguration(appendersByLogLevel.Select(i => i.ToArray()))
        {
            LogMessagePoolExhaustionStrategy = effectiveLogMessagePoolExhaustionStrategy
        };

        IEnumerable<ILoggerConfiguration> GetOrderedLoggerConfigurations()
        {
            var loggerNameWithPeriod = $"{loggerName}.";

            return configuration.Loggers
                                .Where(i => loggerNameWithPeriod.StartsWith(i.NameWithPeriod, StringComparison.Ordinal))
                                .OrderBy(i => i.Name.Length)
                                .Cast<ILoggerConfiguration>()
                                .Prepend(configuration.RootLogger);
        }

        void ApplyLoggerConfig(ILoggerConfiguration loggerConfig)
        {
            effectiveLevel = loggerConfig.Level ?? effectiveLevel;
            effectiveLogMessagePoolExhaustionStrategy = loggerConfig.LogMessagePoolExhaustionStrategy ?? effectiveLogMessagePoolExhaustionStrategy;

            if (!loggerConfig.IncludeParentAppenders)
            {
                foreach (var appenders in appendersByLogLevel)
                    appenders.Clear();
            }

            foreach (var appenderRef in loggerConfig.Appenders)
            {
                var startLevel = Math.Max((int)appenderRef.Level, (int)appenderRef.Appender.Level);

                for (var level = startLevel; level < (int)LogLevel.None; ++level)
                    appendersByLogLevel[level].Add(appenderRef.Appender);
            }
        }
    }

    internal static ResolvedLoggerConfiguration SingleAppender(LogLevel level, Appender? appender = null)
    {
        // For unit tests

        var appenderArray = new[] { appender ?? new NoopAppender() };

        var appendersByLogLevel = Enumerable.Range(0, _levelCount)
                                            .Select(i => i >= (int)level ? appenderArray : Array.Empty<Appender>());

        return new ResolvedLoggerConfiguration(appendersByLogLevel);
    }
}
