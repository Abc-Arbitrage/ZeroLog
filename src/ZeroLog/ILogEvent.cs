using System;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog
{
    public interface ILogEvent
    {
        Level Level { get; }
        DateTime Timestamp { get; }
        int ThreadId { get; }
        string Name { get; }
        ILogEvent Append(string s);
        unsafe ILogEvent Append(byte[] bytes, int length, Encoding encoding);
        ILogEvent AppendAsciiString(byte[] bytes, int length);
        unsafe ILogEvent AppendAsciiString(byte* bytes, int length);
        ILogEvent Append(bool b);
        ILogEvent Append(byte b);
        ILogEvent Append(byte b, string format);
        ILogEvent Append(char c);
        ILogEvent Append(short s);
        ILogEvent Append(short s, string format);
        ILogEvent Append(int i);
        ILogEvent Append(int i, string format);
        ILogEvent Append(long l);
        ILogEvent Append(long l, string format);
        ILogEvent Append(float f);
        ILogEvent Append(float f, string format);
        ILogEvent Append(double d);
        ILogEvent Append(double d, string format);
        ILogEvent Append(decimal d);
        ILogEvent Append(decimal d, string format);
        ILogEvent Append(Guid g);
        ILogEvent Append(Guid g, string format);
        ILogEvent Append(DateTime dt);
        ILogEvent Append(DateTime dt, string format);
        ILogEvent Append(TimeSpan ts);
        ILogEvent Append(TimeSpan ts, string format);
        void Log();
        unsafe void WriteToStringBuffer(StringBuffer stringBuffer);
    }
}