using System.Diagnostics.CodeAnalysis;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal static class SpecialLogEvents
    {
        private static readonly SpecialLogEvent _exhaustedPoolEvent = new SpecialLogEvent(Level.Fatal, "Log message skipped due to LogEvent pool exhaustion.");
        public static IInternalLogEvent ExhaustedPoolEvent => _exhaustedPoolEvent.LogEvent;

        private unsafe class SpecialLogEvent
        {
            [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
            private readonly SafeHeapHandle _bufferHandle;

            public readonly IInternalLogEvent LogEvent;

            public SpecialLogEvent(Level level, string message)
            {
                var log = new Log(null, nameof(ZeroLog));

                _bufferHandle = new SafeHeapHandle(1024);
                var bufferPointer = (byte*)_bufferHandle.DangerousGetHandle();

                var bufferSegment = new BufferSegment(bufferPointer, _bufferHandle.ByteLength);

                LogEvent = new LogEvent(bufferSegment);
                LogEvent.Initialize(level, log);
                LogEvent.Append(message);
            }
        }
    }
}
