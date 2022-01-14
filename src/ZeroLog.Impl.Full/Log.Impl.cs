using System;
using System.Runtime.CompilerServices;
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

    private partial LogMessage InternalAcquireLogMessage(Level level)
    {
        var provider = _logMessageProvider;

        var logMessage = provider is not null
            ? provider.TryAcquireLogMessage() ?? AcquireLogMessageOnExhaustedPool()
            : LogMessage.Empty;

        logMessage.Initialize(this, level);
        return logMessage;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private LogMessage AcquireLogMessageOnExhaustedPool()
    {
        var provider = _logMessageProvider;
        if (provider is null)
            return LogMessage.Empty;

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

                    var message = provider.TryAcquireLogMessage();
                    if (message is not null)
                        return message;
                }
            }

            default:
                return LogMessage.Empty;
        }
    }

    internal void Submit(LogMessage message)
        => _logMessageProvider?.Submit(message);
}
