namespace ZeroLog;

internal interface ILogMessageProvider
{
    LogMessage? TryAcquireLogMessage();
    void Submit(LogMessage message);
}
