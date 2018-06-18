using System;
using System.Text.Formatting;
using ExtraConstraints;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal partial class ForwardingLogEvent : IInternalLogEvent
    {
        private readonly IInternalLogEvent _logEventToAppend;

        public Level Level => _logEventToAppend?.Level ?? default(Level);
        public DateTime Timestamp => default(DateTime);
        public int ThreadId => 0;
        public string Name => _logEventToAppend?.Name;
        public IAppender[] Appenders => _log?.Appenders;

        private Log _log;

        public ForwardingLogEvent(IInternalLogEvent logEventToAppend)
        {
            _logEventToAppend = logEventToAppend;
        }

        public void Initialize(Level level, Log log)
        {
            _log = log;
        }

        public void AppendFormat(string format)
        {
        }

        public void AppendGeneric<T>(T arg)
        {
        }

        public ILogEvent Append(string s) => this;
        public ILogEvent AppendAsciiString(byte[] bytes, int length) => this;
        public unsafe ILogEvent AppendAsciiString(byte* bytes, int length) => this;

        public ILogEvent AppendEnum<[EnumConstraint] T>(T value)
            where T : struct
        {
            return this;
        }

        public ILogEvent AppendEnum<[EnumConstraint] T>(T? value)
            where T : struct
        {
            return this;
        }

        public void Log()
        {
            if (_logEventToAppend != null)
                _log?.Enqueue(_logEventToAppend);
        }

        public void WriteToStringBuffer(StringBuffer stringBuffer)
        {
        }

        public void SetTimestamp(DateTime timestamp)
        {
        }

        public void WriteToStringBufferUnformatted(StringBuffer stringBuffer)
        {
        }

        public bool IsPooled => false;
    }
}
