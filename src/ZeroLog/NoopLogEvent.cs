using System;
using System.Text.Formatting;
using ZeroLog.Appenders;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal partial class NoopLogEvent : IInternalLogEvent
    {
        public static NoopLogEvent Instance { get; } = new NoopLogEvent();

        public Level Level => default(Level);
        public DateTime Timestamp => default(DateTime);
        public int ThreadId => 0;
        public string Name => null;
        public IAppender[] Appenders { get; } = ArrayUtil.Empty<IAppender>();

        private NoopLogEvent()
        {
        }

        public void Initialize(Level level, Log log)
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

        public void Log()
        {
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
