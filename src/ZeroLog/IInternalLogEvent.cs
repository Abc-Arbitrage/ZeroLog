using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    internal interface IInternalLogEvent : ILogEvent
    {
        void Initialize(Level level, Log log, LogEventArgumentExhaustionStrategy argumentExhaustionStrategy);
        void SetTimestamp(DateTime timestamp);
        void AppendFormat(string format);
        void AppendGeneric<T>(T arg);
        void WriteToStringBuffer(StringBuffer stringBuffer, IList<IntPtr>? keyValuePtrList = null);
        void WriteToStringBufferUnformatted(StringBuffer stringBuffer);
        bool IsPooled { get; }
    }
}
