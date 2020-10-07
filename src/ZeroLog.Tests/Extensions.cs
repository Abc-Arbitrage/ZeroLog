using System.Text.Formatting;

namespace ZeroLog.Tests
{
    internal static class Extensions
    {
        public static void WriteToStringBuffer(this IInternalLogEvent logEvent, StringBuffer stringBuffer)
            => logEvent.WriteToStringBuffer(stringBuffer, new KeyValuePointerBuffer());
    }
}
