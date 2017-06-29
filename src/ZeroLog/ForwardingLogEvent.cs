using System;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog
{
    internal class ForwardingLogEvent : IInternalLogEvent
    {
        private readonly IInternalLogEvent _logEventToAppend;

        public Level Level { get; }
        public DateTime Timestamp { get; }
        public int ThreadId { get; }
        public string Name { get; }

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
        public ILogEvent Append(byte[] bytes, int length, Encoding encoding) => this;
        public ILogEvent AppendAsciiString(byte[] bytes, int length) => this;
        public unsafe ILogEvent AppendAsciiString(byte* bytes, int length) => this;
        public ILogEvent Append(bool b) => this;
        public ILogEvent Append(byte b) => this;
        public ILogEvent Append(byte b, string format) => this;
        public ILogEvent Append(char c) => this;
        public ILogEvent Append(short s) => this;
        public ILogEvent Append(short s, string format) => this;
        public ILogEvent Append(int i) => this;
        public ILogEvent Append(int i, string format) => this;
        public ILogEvent Append(long l) => this;
        public ILogEvent Append(long l, string format) => this;
        public ILogEvent Append(float f) => this;
        public ILogEvent Append(float f, string format) => this;
        public ILogEvent Append(double d) => this;
        public ILogEvent Append(double d, string format) => this;
        public ILogEvent Append(decimal d) => this;
        public ILogEvent Append(decimal d, string format) => this;
        public ILogEvent Append(Guid g) => this;
        public ILogEvent Append(Guid g, string format) => this;
        public ILogEvent Append(DateTime dt) => this;
        public ILogEvent Append(DateTime dt, string format) => this;
        public ILogEvent Append(TimeSpan ts) => this;
        public ILogEvent Append(TimeSpan ts, string format) => this;

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
    }
}
