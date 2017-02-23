using System;

namespace ZeroLog
{
    internal interface IInternalLogEvent : ILogEvent
    {
        void Initialize(Level level, Log log);
        void SetTimestamp(DateTime timestamp);
        void AppendFormat(string format);
        void AppendGeneric<T>(T arg);
    }
}