using System;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public partial interface ILogEvent : ILogEventHeader
    {
        IAppender[] Appenders { get; }

        ILogEvent Append(string? s);
        ILogEvent AppendAsciiString(byte[]? bytes, int length);
        unsafe ILogEvent AppendAsciiString(byte* bytes, int length);
        ILogEvent AppendAsciiString(ReadOnlySpan<byte> bytes);
        ILogEvent AppendAsciiString(ReadOnlySpan<char> chars);
        ILogEvent AppendKeyValueAscii(string key, byte[]? bytes, int length);
        unsafe ILogEvent AppendKeyValueAscii(string key, byte* bytes, int length);
        ILogEvent AppendKeyValueAscii(string key, ReadOnlySpan<byte> bytes);
        ILogEvent AppendKeyValueAscii(string key, ReadOnlySpan<char> chars);

        ILogEvent AppendEnum<T>(T value)
            where T : struct, Enum;

        ILogEvent AppendEnum<T>(T? value)
            where T : struct, Enum;

        ILogEvent AppendKeyValue(string key, string? value);

        ILogEvent AppendKeyValue<T>(string key, T value)
            where T : struct, Enum;

        ILogEvent AppendKeyValue<T>(string key, T? value)
            where T : struct, Enum;

        void Log();
    }
}
