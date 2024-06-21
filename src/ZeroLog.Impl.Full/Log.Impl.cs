using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog;

partial class Log
{
    private ILogMessageProvider? _logMessageProvider;

    internal ResolvedLoggerConfiguration Config { get; private set; } = ResolvedLoggerConfiguration.Empty;

    internal void UpdateConfiguration(ILogMessageProvider? provider, ZeroLogConfiguration? config)
        => UpdateConfiguration(provider, config?.ResolveLoggerConfiguration(Name));

    internal void UpdateConfiguration(ILogMessageProvider? provider, ResolvedLoggerConfiguration? config)
    {
        _logMessageProvider = provider;
        Config = config ?? ResolvedLoggerConfiguration.Empty;
        _logLevel = Config.Level;
    }

    internal void DisableLogging()
    {
        _logMessageProvider = null;
        _logLevel = LogLevel.None;
    }

    public partial bool IsEnabled(LogLevel level)
        => level >= _logLevel;

    internal Appender[] GetAppenders(LogLevel level)
        => Config.GetAppenders(level);

    private partial LogMessage InternalAcquireLogMessage(LogLevel level)
    {
        var provider = _logMessageProvider;

        var logMessage = provider is not null
            ? provider.AcquireLogMessage(Config.LogMessagePoolExhaustionStrategy)
            : LogMessage.Empty;

        logMessage.Initialize(this, level);
        return logMessage;
    }

    internal void Submit(LogMessage message)
        => _logMessageProvider?.Submit(message);
}
