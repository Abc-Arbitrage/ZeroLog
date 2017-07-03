using System;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal class BufferSegmentProvider : IDisposable
    {
        private readonly LargeBuffer[] _largeBuffers = new LargeBuffer[16384];

        private readonly int _largeBufferSize;
        private readonly int _bufferSegmentSize;

        public BufferSegmentProvider(int largeBufferSize, int bufferSegmentSize)
        {
            _largeBufferSize = largeBufferSize;
            _bufferSegmentSize = bufferSegmentSize;

            AllocateLargeBuffer(0);
        }

        public int LastSegmentIndex { get; private set; } = -1;
        public int LargeBufferCount { get; private set; }

        public BufferSegment GetSegment()
        {
            var nextSegmentIndex = ++LastSegmentIndex;

            var largeBuffer = AllocateLargeBufferIfNeeded(nextSegmentIndex);

            var bufferSegmentIndex = nextSegmentIndex - largeBuffer.FirstSegmentGlobalIndex;

            // ReSharper disable once InconsistentlySynchronizedField
            var bufferSegment = largeBuffer.GetSegment(bufferSegmentIndex * _bufferSegmentSize, _bufferSegmentSize);

            return bufferSegment;
        }

        private LargeBuffer AllocateLargeBufferIfNeeded(int segmentIndex)
        {
            var largeBufferIndex = segmentIndex * _bufferSegmentSize / _largeBufferSize;

            // ReSharper disable once InconsistentlySynchronizedField
            if (largeBufferIndex < LargeBufferCount)
                return _largeBuffers[largeBufferIndex];

            if (largeBufferIndex < LargeBufferCount)
                return _largeBuffers[largeBufferIndex];

            return AllocateLargeBuffer(segmentIndex);
        }

        private LargeBuffer AllocateLargeBuffer(int firstSegmentGlobalIndex)
        {
            var largeBuffer = new LargeBuffer(_largeBufferSize, firstSegmentGlobalIndex);
            _largeBuffers[LargeBufferCount++] = largeBuffer;
            return largeBuffer;
        }

        public void Dispose()
        {
            for (var i = 0; i < LargeBufferCount; ++i)
                _largeBuffers[i].Dispose();
        }

        private unsafe class LargeBuffer : IDisposable
        {
            private readonly SafeHeapHandle _bufferHandle;
            private readonly byte* _bufferPointer;

            public LargeBuffer(int size, int firstSegmentGlobalIndex)
            {
                FirstSegmentGlobalIndex = firstSegmentGlobalIndex;

                _bufferHandle = new SafeHeapHandle(size);
                _bufferPointer = (byte*) _bufferHandle.DangerousGetHandle();
            }

            public int FirstSegmentGlobalIndex { get; }

            public BufferSegment GetSegment(int offset, int length)
            {
                if (offset < 0 || offset >= _bufferHandle.ByteLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (length <= 0 || offset + length > _bufferHandle.ByteLength)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return new BufferSegment(_bufferPointer + offset, length);
            }

            public void Dispose()
            {
                _bufferHandle.Dispose();
            }
        }
    }
}