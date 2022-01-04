using System;
using ZeroLog.Appenders;

namespace ZeroLog;

public sealed partial class Log : ILog
{
    private readonly IInternalLogManager _logManager;
    private readonly LogMessage _poolExhaustedMessage = new("Log message skipped due to pool exhaustion.");
    private Level _logLevel;

    internal string Name { get; }

    internal IAppender[] Appenders { get; private set; } = Array.Empty<IAppender>();
    internal LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; private set; }
    internal LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; private set; }

    internal Log(IInternalLogManager logManager, string name)
    {
        Name = name;
        _logManager = logManager;

        ResetConfiguration();
    }

    internal void ResetConfiguration()
    {
        var config = _logManager?.ResolveLogConfig(Name) ?? default;

        Appenders = config.Appenders;
        LogEventPoolExhaustionStrategy = config.LogEventPoolExhaustionStrategy;
        LogEventArgumentExhaustionStrategy = config.LogEventArgumentExhaustionStrategy;
        _logLevel = config.Level;
    }

    public bool IsEnabled(Level level)
        => level >= _logLevel;

    public LogMessage ForLevel(Level level)
        => IsEnabled(level) ? GetLogMessage(level) : LogMessage.Empty;

    private LogMessage GetLogMessage(Level level)
    {
        var logMessage = _logManager.AcquireLogMessage(LogEventPoolExhaustionStrategy) ?? _poolExhaustedMessage;
        logMessage.Initialize(this, level);

        return logMessage;
    }

    internal void Enqueue(LogMessage message)
        => _logManager.Enqueue(message);

    internal static Log CreateEmpty(string name)
        => new(EmptyLogManager.Instance, name);

    private class EmptyLogManager : IInternalLogManager
    {
        public static EmptyLogManager Instance { get; } = new();

        public Level Level => Level.Fatal;

        public void Dispose()
        {
        }

        public LogMessage? AcquireLogMessage(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy)
            => LogMessage.Empty;

        public void Enqueue(LogMessage logEvent)
        {
        }

        public LogConfig ResolveLogConfig(string name)
            => default;
    }
}
