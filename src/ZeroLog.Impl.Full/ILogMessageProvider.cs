using ZeroLog.Configuration;

namespace ZeroLog;

internal interface ILogMessageProvider
{
    LogMessage AcquireLogMessage(LogMessagePoolExhaustionStrategy poolExhaustionStrategy);
    void Submit(LogMessage message);
}
