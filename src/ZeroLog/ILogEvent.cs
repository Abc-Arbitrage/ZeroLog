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
        LogEvent Append(string s);
        unsafe LogEvent Append(byte[] bytes, int length, Encoding encoding);
        LogEvent AppendAsciiString(byte[] bytes, int length);
        unsafe LogEvent AppendAsciiString(byte* bytes, int length);
        LogEvent Append(bool b);
        LogEvent Append(byte b);
        LogEvent Append(byte b, string format);
        LogEvent Append(char c);
        LogEvent Append(short s);
        LogEvent Append(short s, string format);
        LogEvent Append(int i);
        LogEvent Append(int i, string format);
        LogEvent Append(long l);
        LogEvent Append(long l, string format);
        LogEvent Append(float f);
        LogEvent Append(float f, string format);
        LogEvent Append(double d);
        LogEvent Append(double d, string format);
        LogEvent Append(decimal d);
        LogEvent Append(decimal d, string format);
        LogEvent Append(Guid g);
        LogEvent Append(Guid g, string format);
        LogEvent Append(DateTime dt);
        LogEvent Append(DateTime dt, string format);
        LogEvent Append(TimeSpan ts);
        LogEvent Append(TimeSpan ts, string format);
        void Log();
        unsafe void WriteToStringBuffer(StringBuffer stringBuffer);
    }
}