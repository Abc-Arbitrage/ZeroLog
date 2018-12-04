using System;

namespace ZeroLog
{
    internal interface IInternalLogManager : ILogManager, IDisposable
    {
        IInternalLogEvent AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent logEvent, Level level, Log log);
        void Enqueue(IInternalLogEvent logEvent);
        ILog GetLog(string name);
        LogConfig ResolveLogConfig(string name);
        BufferSegment GetBufferSegment();
    }
}
