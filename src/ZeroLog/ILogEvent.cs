using System;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public partial interface ILogEvent : ILogEventHeader
    {
        IAppender[] Appenders { get; }

        ILogEvent Append(string s);
        ILogEvent AppendF(string s);
        ILogEvent AppendAsciiString(byte[] bytes, int length);
        unsafe ILogEvent AppendAsciiString(byte* bytes, int length);

        ILogEvent AppendEnum<T>(T value)
            where T : struct, Enum;

        ILogEvent AppendEnum<T>(T? value)
            where T : struct, Enum;

        ILogEvent AppendUnmanaged<T>(T value) where T : unmanaged;

        void Log();
    }
}
