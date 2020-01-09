using System;
using JetBrains.Annotations;

namespace ZeroLog
{
    internal interface IInternalLogManager : ILogManager, IDisposable
    {
        IInternalLogEvent? AcquireLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy);

        void Enqueue(IInternalLogEvent logEvent);
        ILog GetLog(string name);
        LogConfig ResolveLogConfig(string name);
        BufferSegment GetBufferSegment();
    }
}
