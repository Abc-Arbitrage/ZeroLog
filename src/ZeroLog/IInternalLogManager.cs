using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZeroLog
{
    internal interface IInternalLogManager : ILogManager
    {
        bool IsRunning { get; set;  }
        Task WriteTask { get; }
        List<IAppender> Appenders { get; }
        LogEvent AllocateLogEvent();
        void Enqueue(LogEvent logEvent);
        ILog GetNewLog(IInternalLogManager logManager, string name);
    }
}