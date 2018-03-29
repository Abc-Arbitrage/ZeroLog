using System;
using System.Collections.Generic;
using ExtraConstraints;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public interface ILogEvent : ILogEventHeader
    {
        IList<IAppender> Appenders { get; }

        ILogEvent Append(string s);
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

        ILogEvent AppendEnum<[EnumConstraint] T>(T value)
            where T : struct;

        void Log();
    }
}
