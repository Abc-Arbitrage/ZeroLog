using System.Runtime.InteropServices;

namespace ZeroLog
{
    internal static class SpecialLogEvents
    {
        private static readonly SpecialLogEvent _exhaustedPoolEvent = new SpecialLogEvent(Level.Fatal, "Log message skipped due to LogEvent pool exhaustion.");
        public static IInternalLogEvent ExhaustedPoolEvent => _exhaustedPoolEvent.LogEvent;

        private unsafe class SpecialLogEvent
        {
            private readonly byte[] _buffer = new byte[1024];
            private readonly GCHandle _bufferHandle;

            public readonly IInternalLogEvent LogEvent;

            public SpecialLogEvent(Level level, string message)
            {
                var log = new Log(null, nameof(ZeroLog));
                var bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                var bufferSegment = new BufferSegment((byte*)bufferHandle.AddrOfPinnedObject(), _buffer.Length);
                LogEvent = new LogEvent(bufferSegment);
                LogEvent.Initialize(level, log);
                LogEvent.Append(message);

                _bufferHandle = bufferHandle;
            }
        }
    }
}
