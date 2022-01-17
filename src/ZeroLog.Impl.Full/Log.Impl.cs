using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog;

partial class Log
{
    private static readonly Appender[][] _emptyAppendersByLogLevel = Enumerable.Repeat(Array.Empty<Appender>(), (int)Level.None + 1).ToArray();

    private readonly LogMessage _poolExhaustedMessage = new("Log message skipped due to pool exhaustion.");

    private ILogMessageProvider? _logMessageProvider;
    private Appender[][] _appendersByLogLevel = _emptyAppendersByLogLevel;
    private LogMessagePoolExhaustionStrategy _logMessagePoolExhaustionStrategy;

    internal void UpdateConfiguration(ILogMessageProvider? provider, ResolvedLoggerConfiguration? config)
    {
        _logMessageProvider = provider;

        _appendersByLogLevel = config?.AppendersByLogLevel ?? _emptyAppendersByLogLevel;
        _logLevel = config?.Level ?? Level.None;
        _logMessagePoolExhaustionStrategy = config?.LogMessagePoolExhaustionStrategy ?? LogMessagePoolExhaustionStrategy.Default;
    }

    public partial bool IsEnabled(Level level)
        => level >= _logLevel;

    internal Appender[] GetAppenders(Level level)
        => _appendersByLogLevel[(int)level];

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
