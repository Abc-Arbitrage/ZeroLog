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
        List<IAppender> Appenders { get; }
        IInternalLogEvent AllocateLogEvent();
        void Enqueue(IInternalLogEvent logEvent);
        ILog GetNewLog(IInternalLogManager logManager, string name);
    }
}
