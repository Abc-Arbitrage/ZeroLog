using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using ZeroLog.Appenders;

namespace ZeroLog.Config;

internal sealed class ResolvedLoggerConfiguration
{
    private const int _levelCount = (int)Level.None + 1;

    public static ResolvedLoggerConfiguration Empty { get; } = new(Enumerable.Repeat(Array.Empty<Appender>(), _levelCount))
    {
        LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage
    };

    private readonly Appender[][] _appendersByLogLevel;

    public Level Level { get; }
    public LogMessagePoolExhaustionStrategy LogMessagePoolExhaustionStrategy { get; private init; } = LogMessagePoolExhaustionStrategy.Default;

    private ResolvedLoggerConfiguration(IEnumerable<Appender[]> appendersByLogLevel)
    {
        _appendersByLogLevel = appendersByLogLevel.ToArray();
        _appendersByLogLevel[(int)Level.None] = Array.Empty<Appender>();
        Debug.Assert(_appendersByLogLevel.Length == _levelCount);

        Level = Level.None;

        for (var level = 0; level < _appendersByLogLevel.Length; ++level)
        {
            if (_appendersByLogLevel[level].Length == 0)
                continue;

            Level = (Level)level;
            break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Appender[] GetAppenders(Level level)
        => _appendersByLogLevel[(int)level];

    public static ResolvedLoggerConfiguration Resolve(string loggerName, ZeroLogConfiguration configuration)
    {
        var appendersByLogLevel = new HashSet<Appender>[_levelCount];

        for (var i = 0; i < appendersByLogLevel.Length; i++)
            appendersByLogLevel[i] = new HashSet<Appender>(ReferenceEqualityComparer.Instance);

        var effectiveLevel = Level.Trace;
        var effectiveLogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Default;

        foreach (var loggerConfig in GetOrderedLoggerConfigurations())
            ApplyLoggerConfig(loggerConfig);

        for (var level = 0; level < (int)effectiveLevel; ++level)
            appendersByLogLevel[level].Clear();

        return new ResolvedLoggerConfiguration(appendersByLogLevel.Select(i => i.ToArray()))
        {
            LogMessagePoolExhaustionStrategy = effectiveLogMessagePoolExhaustionStrategy
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

                for (var level = startLevel; level < (int)Level.None; ++level)
                    appendersByLogLevel[level].Add(appenderRef.Appender);
            }
        }
    }

    internal static ResolvedLoggerConfiguration SingleAppender(Level level, Appender? appender = null)
    {
        // For unit tests

        var appenderArray = new[] { appender ?? new NoopAppender() };

        var appendersByLogLevel = Enumerable.Range(0, _levelCount)
                                            .Select(i => i >= (int)level ? appenderArray : Array.Empty<Appender>());

        return new ResolvedLoggerConfiguration(appendersByLogLevel);
    }
}
