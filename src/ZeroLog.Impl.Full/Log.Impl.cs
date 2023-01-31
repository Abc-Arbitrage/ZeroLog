using ZeroLog.Appenders;
using ZeroLog.Configuration;

namespace ZeroLog;

partial class Log
{
    private ILogMessageProvider? _logMessageProvider;
    private ResolvedLoggerConfiguration _config = ResolvedLoggerConfiguration.Empty;

    internal void UpdateConfiguration(ILogMessageProvider? provider, ZeroLogConfiguration? config)
        => UpdateConfiguration(provider, config?.ResolveLoggerConfiguration(Name));

    internal void UpdateConfiguration(ILogMessageProvider? provider, ResolvedLoggerConfiguration? config)
    {
        _logMessageProvider = provider;
        _config = config ?? ResolvedLoggerConfiguration.Empty;
        _logLevel = _config.Level;
    }

    public partial bool IsEnabled(LogLevel level)
        => level >= _logLevel;

    internal Appender[] GetAppenders(LogLevel level)
        => _config.GetAppenders(level);

    private partial LogMessage InternalAcquireLogMessage(LogLevel level)
    {
        var provider = _logMessageProvider;

        var logMessage = provider is not null
            ? provider.AcquireLogMessage(_config.LogMessagePoolExhaustionStrategy)
            : LogMessage.Empty;

        logMessage.Initialize(this, level);
        return logMessage;
    }

    internal void Submit(LogMessage message)
        => _logMessageProvider?.Submit(message);
}
