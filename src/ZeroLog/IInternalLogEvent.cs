using System;
using System.Text.Formatting;

namespace ZeroLog
{
    internal interface IInternalLogEvent : ILogEvent
    {
        void Initialize(Level level, Log log);
        void SetTimestamp(DateTime timestamp);
        void AppendFormat(string format);
        void AppendGeneric<T>(T arg);
        void WriteToStringBuffer(StringBuffer stringBuffer);
        void WriteToStringBufferUnformatted(StringBuffer stringBuffer);
    }
}