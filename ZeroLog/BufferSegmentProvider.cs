using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZeroLog
{
    public class BufferSegmentProvider : IDisposable
    {
        private readonly LargeBuffer[] _largeBuffers = new LargeBuffer[16384];

        private readonly int _largeBufferSize;
        private readonly int _bufferSegmentSize;

        private int _segmentIndex = -1;

        public BufferSegmentProvider(int largeBufferSize, int bufferSegmentSize)
        {
            _largeBufferSize = largeBufferSize;
            _bufferSegmentSize = bufferSegmentSize;

            AllocateLargeBuffer(0);
        }

        public int LastSegmentIndex => _segmentIndex;
        public int LargeBufferCount { get; private set; }

        public BufferSegment GetSegment()
        {
            var nextSegmentIndex = Interlocked.Increment(ref _segmentIndex);

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

            lock (_largeBuffers)
            {
                if (largeBufferIndex < LargeBufferCount)
                    return _largeBuffers[largeBufferIndex];

                return AllocateLargeBuffer(segmentIndex);
            }
        }

        private LargeBuffer AllocateLargeBuffer(int firstSegmentGlobalIndex)
        {
            var largeBuffer = new LargeBuffer(_largeBufferSize, firstSegmentGlobalIndex);
            _largeBuffers[LargeBufferCount++] = largeBuffer;
            return largeBuffer;
        }

        public void Dispose()
        {
            foreach (var pinnedBuffer in _largeBuffers)
            {
                pinnedBuffer.Dispose();
            }
        }

        private unsafe class LargeBuffer : IDisposable
        {
            private readonly int _firstSegmentGlobalIndex;
            private GCHandle _handle;
            private readonly byte[] _buffer;
            private readonly byte* _bufferPointer;

            public LargeBuffer(int size, int firstSegmentGlobalIndex)
            {
                _firstSegmentGlobalIndex = firstSegmentGlobalIndex;
                _buffer = new byte[size];
                _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                _bufferPointer = (byte*)_handle.AddrOfPinnedObject().ToPointer();
            }

            public int FirstSegmentGlobalIndex => _firstSegmentGlobalIndex;

            public BufferSegment GetSegment(int offset, int length)
            {
                if (offset < 0 || offset >= _buffer.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                if (length <= 0 || offset + length > _buffer.Length)
                    throw new ArgumentOutOfRangeException(nameof(offset));

                return new BufferSegment(_bufferPointer + offset, length);
            }

            public void Dispose()
            {
                if (_handle.IsAllocated)
                    _handle.Free();
            }
        }
    }
}
