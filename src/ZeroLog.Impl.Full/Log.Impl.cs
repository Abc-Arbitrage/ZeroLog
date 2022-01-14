using System;
using System.Threading;
using ZeroLog.Appenders;

namespace ZeroLog;

partial class Log
{
    private readonly LogMessage _poolExhaustedMessage = new("Log message skipped due to pool exhaustion.");

    private ILogMessageProvider? _logMessageProvider;
    private LogMessagePoolExhaustionStrategy _logMessagePoolExhaustionStrategy;

    internal IAppender[] Appenders { get; private set; } = Array.Empty<IAppender>();

    internal void UpdateConfiguration(ILogMessageProvider? provider, LogConfig config)
    {
        _logMessageProvider = provider;

        Appenders = config.Appenders ?? Array.Empty<IAppender>();
        _logMessagePoolExhaustionStrategy = config.LogMessagePoolExhaustionStrategy;
        _logLevel = config.Level;
    }

    public partial bool IsEnabled(Level level)
        => level >= _logLevel;

    public partial LogMessage ForLevel(Level level)
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
