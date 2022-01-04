using System;

namespace ZeroLog;

internal interface IInternalLogManager : ILogManager, IDisposable
{
    LogMessage? AcquireLogMessage(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy);

    void Enqueue(LogMessage logEvent);
    LogConfig ResolveLogConfig(string name);
}
