using System;
using System.Text.Formatting;
using JetBrains.Annotations;
using ZeroLog.Appenders;

namespace ZeroLog
{
    internal partial class ForwardingLogEvent : IInternalLogEvent
    {
        private readonly IInternalLogEvent _logEventToAppend;
        private readonly Log _log;

        public Level Level => _logEventToAppend.Level;
        public DateTime Timestamp => default;
        public int ThreadId => 0;
        public string Name => _logEventToAppend.Name;
        public IAppender[] Appenders => _log.Appenders;

        public ForwardingLogEvent([NotNull] IInternalLogEvent logEventToAppend, [NotNull] Log log)
        {
            _logEventToAppend = logEventToAppend;
            _log = log;
        }

        public void Initialize(Level level, Log log, LogEventArgumentExhaustionStrategy argumentExhaustionStrategy)
        {
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

        public ILogEvent AppendEnum<T>(T value)
            where T : struct, Enum
        {
            return this;
        }

        public ILogEvent AppendEnum<T>(T? value)
            where T : struct, Enum
        {
            return this;
        }

        public ILogEvent AppendUnmanaged<T>(T value) where T : unmanaged
        {
            return this;
        }

        public ILogEvent AppendUnmanaged<T>(ref T value) where T : unmanaged
        {
            return this;
        }

        public ILogEvent AppendUnmanaged<T>(T? value) where T : unmanaged
        {
            return this;
        }

        public ILogEvent AppendUnmanaged<T>(ref T? value) where T : unmanaged
        {
            return this;
        }

        public void Log()
        {
            _log.Enqueue(_logEventToAppend);
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

        public ILogEvent AppendF(string s)
        {
            return this;
        }

        public bool IsPooled => false;
    }
}
