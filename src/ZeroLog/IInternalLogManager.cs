using System;
using System.Collections.Generic;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal interface IInternalLogManager : ILogManager, IDisposable
    {
        IInternalLogEvent AllocateLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent logEvent, Level level, Log log);
        void Enqueue(IInternalLogEvent logEvent);
        ILog GetLog(string name);
        IList<IAppender> ResolveAppenders(string name);
        LogEventPoolExhaustionStrategy ResolveLogEventPoolExhaustionStrategy(string name);
        Level ResolveLevel(string name);
        BufferSegment GetBufferSegment();
    }
}
