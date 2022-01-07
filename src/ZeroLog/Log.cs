using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Appenders;

namespace ZeroLog;

public sealed partial class Log
{
    private readonly LogMessage _poolExhaustedMessage = new("Log message skipped due to pool exhaustion.");

    private ILogMessageProvider? _logMessageProvider;
    private Level _logLevel;
    private LogMessagePoolExhaustionStrategy _logMessagePoolExhaustionStrategy;

    internal string Name { get; }

    internal IAppender[] Appenders { get; private set; } = Array.Empty<IAppender>();

    internal Log(string name)
    {
        Name = name;
    }

    internal void UpdateConfiguration(ILogMessageProvider? provider, LogConfig config)
    {
        _logMessageProvider = provider;

        Appenders = config.Appenders ?? Array.Empty<IAppender>();
        _logMessagePoolExhaustionStrategy = config.LogMessagePoolExhaustionStrategy;
        _logLevel = config.Level;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEnabled(Level level)
        => level >= _logLevel;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage ForLevel(Level level)
        => IsEnabled(level)
            ? GetLogMessage(level)
            : LogMessage.Empty;

    private LogMessage GetLogMessage(Level level)
    {
        var logMessage = AcquireLogMessage();
        logMessage.Initialize(this, level);
        return logMessage;

        LogMessage AcquireLogMessage()
        {
            var provider = _logMessageProvider;
            if (provider is null)
                return LogMessage.Empty;

            var message = provider.TryAcquireLogMessage();
            if (message is not null)
                return message;

            switch (_logMessagePoolExhaustionStrategy)
            {
                case LogMessagePoolExhaustionStrategy.DropLogMessageAndNotifyAppenders:
                    return _poolExhaustedMessage;

                case LogMessagePoolExhaustionStrategy.DropLogMessage:
                    return LogMessage.Empty;

                case LogMessagePoolExhaustionStrategy.WaitUntilAvailable:
                {
                    var spinWait = new SpinWait();

                    while (true)
                    {
                        spinWait.SpinOnce();

                        message = provider.TryAcquireLogMessage();
                        if (message is not null)
                            return message;
                    }
                }

                default:
                    return LogMessage.Empty;
            }
        }
    }

    internal void Submit(LogMessage message)
        => _logMessageProvider?.Submit(message);
}
