using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal interface IInternalLogManager : ILogManager, IDisposable
    {
        bool IsRunning { get; set; }
        Task WriteTask { get; }
        IInternalLogEvent AllocateLogEvent(LogEventPoolExhaustionStrategy logEventPoolExhaustionStrategy, IInternalLogEvent logEvent);
        void Enqueue(IInternalLogEvent logEvent);
        ILog GetNewLog(IInternalLogManager logManager, string name);
        IList<IAppender> ResolveAppenders(string name);
        LogEventPoolExhaustionStrategy ResolveLogEventPoolExhaustionStrategy(string name);
        Level ResolveLevel(string name);
    }
}
