using System;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog
{
    internal class NoopLogEvent : IInternalLogEvent
    {
        public Level Level { get; }
        public DateTime Timestamp { get; }
        public int ThreadId { get; }
        public string Name { get; }

        private Log _log;

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

        public ILogEvent Append(string s)
        {
            return this;
        }

        public ILogEvent Append(byte[] bytes, int length, Encoding encoding)
        {
            return this;
        }

        public ILogEvent AppendAsciiString(byte[] bytes, int length)
        {
            return this;
        }

        public unsafe ILogEvent AppendAsciiString(byte* bytes, int length)
        {
            return this;
        }

        public ILogEvent Append(bool b)
        {
            return this;
        }

        public ILogEvent Append(byte b)
        {
            return this;
        }

        public ILogEvent Append(byte b, string format)
        {
            return this;
        }

        public ILogEvent Append(char c)
        {
            return this;
        }

        public ILogEvent Append(short s)
        {
            return this;
        }

        public ILogEvent Append(short s, string format)
        {
            return this;
        }

        public ILogEvent Append(int i)
        {
            return this;
        }

        public ILogEvent Append(int i, string format)
        {
            return this;
        }

        public ILogEvent Append(long l)
        {
            return this;
        }

        public ILogEvent Append(long l, string format)
        {
            return this;
        }

        public ILogEvent Append(float f)
        {
            return this;
        }

        public ILogEvent Append(float f, string format)
        {
            return this;
        }

        public ILogEvent Append(double d)
        {
            return this;
        }

        public ILogEvent Append(double d, string format)
        {
            return this;
        }

        public ILogEvent Append(decimal d)
        {
            return this;
        }

        public ILogEvent Append(decimal d, string format)
        {
            return this;
        }

        public ILogEvent Append(Guid g)
        {
            return this;
        }

        public ILogEvent Append(Guid g, string format)
        {
            return this;
        }

        public ILogEvent Append(DateTime dt)
        {
            return this;
        }

        public ILogEvent Append(DateTime dt, string format)
        {
            return this;
        }

        public ILogEvent Append(TimeSpan ts)
        {
            return this;
        }

        public ILogEvent Append(TimeSpan ts, string format)
        {
            return this;
        }

        public void Log()
        {
            _log.Enqueue(SpecialLogEvents.ExhaustedPoolEvent);
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
