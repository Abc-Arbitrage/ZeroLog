namespace ZeroLog;

internal interface ILogMessageProvider
{
    LogMessage? AcquireLogMessage(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy);
    void Enqueue(LogMessage logEvent);
}
