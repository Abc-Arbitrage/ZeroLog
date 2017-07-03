using System.Diagnostics.CodeAnalysis;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal static class SpecialLogEvents
    {
        internal unsafe class SpecialLogEvent
        {
            [SuppressMessage("ReSharper", "PrivateFieldCanBeConvertedToLocalVariable")]
            private readonly SafeHeapHandle _bufferHandle;

            public readonly IInternalLogEvent LogEvent;

            public SpecialLogEvent(Level level, string message, Log log)
            {
                _bufferHandle = new SafeHeapHandle(1024);
                var bufferPointer = (byte*)_bufferHandle.DangerousGetHandle();

                var bufferSegment = new BufferSegment(bufferPointer, _bufferHandle.ByteLength);

                LogEvent = new UnpooledLogEvent(bufferSegment);
                LogEvent.Initialize(level, log);
                LogEvent.Append(message);
            }
        }
    }
}
