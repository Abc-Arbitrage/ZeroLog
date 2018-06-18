using ExtraConstraints;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public partial interface ILogEvent : ILogEventHeader
    {
        IAppender[] Appenders { get; }

        ILogEvent Append(string s);
        ILogEvent AppendAsciiString(byte[] bytes, int length);
        unsafe ILogEvent AppendAsciiString(byte* bytes, int length);

        ILogEvent AppendEnum<[EnumConstraint] T>(T value)
            where T : struct;

        ILogEvent AppendEnum<[EnumConstraint] T>(T? value)
            where T : struct;

        void Log();
    }
}
