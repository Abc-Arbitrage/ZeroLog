using System;
using System.Collections.Generic;
using System.Text.Formatting;

#if false

namespace ZeroLog
{
    internal interface IInternalLogEvent : ILogEvent
    {
        void Initialize(Level level, Log log, LogEventArgumentExhaustionStrategy argumentExhaustionStrategy);
        void SetTimestamp(DateTime timestamp);
        void WriteToStringBuffer(StringBuffer stringBuffer, KeyValuePointerBuffer keyValuePointerBuffer);
        void WriteToStringBufferUnformatted(StringBuffer stringBuffer);
        bool IsPooled { get; }
    }
}

#endif
