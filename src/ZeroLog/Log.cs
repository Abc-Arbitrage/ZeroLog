using System;
using ZeroLog.Appenders;

namespace ZeroLog;

public sealed partial class Log : ILog
{
    private readonly LogMessage _poolExhaustedMessage = new("Log message skipped due to pool exhaustion.");

    private ILogMessageProvider? _logMessageProvider;
    private Level _logLevel;

    internal string Name { get; }

    internal IAppender[] Appenders { get; private set; } = Array.Empty<IAppender>();
    internal LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy { get; private set; }
    internal LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy { get; private set; }

    internal Log(string name)
    {
        Name = name;
    }

    internal void UpdateConfiguration(ILogMessageProvider? provider, LogConfig config)
    {
        _logMessageProvider = provider;

        Appenders = config.Appenders ?? Array.Empty<IAppender>();
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
        var provider = _logMessageProvider;

        if (provider is null)
            return LogMessage.Empty;

        var logMessage = provider.AcquireLogMessage(LogEventPoolExhaustionStrategy) ?? _poolExhaustedMessage;
        logMessage.Initialize(this, level);

        return logMessage;
    }

    internal void Enqueue(LogMessage message)
        => _logMessageProvider?.Enqueue(message);
}
