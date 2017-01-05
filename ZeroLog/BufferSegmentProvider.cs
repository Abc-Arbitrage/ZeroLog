using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZeroLog
{
    public class BufferSegmentProvider : IDisposable
    {
        private readonly List<LargeBuffer> _pinnedBuffers = new List<LargeBuffer>();
        private readonly int _largeBufferSize;
        private readonly int _bufferSegmentSize;

        private int _segmentIndex = -1;

        public BufferSegmentProvider(int largeBufferSize, int bufferSegmentSize)
        {
            _largeBufferSize = largeBufferSize;
            _bufferSegmentSize = bufferSegmentSize;

            AllocateLargeBuffer();
        }

        public int LastSegmentIndex => _segmentIndex;
        public int LargeBufferCount => _pinnedBuffers.Count;

        public BufferSegment GetSegment()
        {
            var nextSegmentIndex = GetNextSegmentIndex();

            var largeBufferIndex = AllocateLargeBufferIfNeeded(nextSegmentIndex);

            var bufferSegmentIndex = nextSegmentIndex - (_largeBufferSize / _bufferSegmentSize * largeBufferIndex);

            // ReSharper disable once InconsistentlySynchronizedField
            var bufferSegment = _pinnedBuffers[largeBufferIndex].GetSegment(bufferSegmentIndex * _bufferSegmentSize, _bufferSegmentSize);

            return bufferSegment;
        }

        private int AllocateLargeBufferIfNeeded(int segmentIndex)
        {
            var largeBufferIndex = segmentIndex * _bufferSegmentSize / _largeBufferSize;
            if (largeBufferIndex < _pinnedBuffers.Count)
                return largeBufferIndex;

            lock (_pinnedBuffers)
            {
                if (largeBufferIndex < _pinnedBuffers.Count)
                    return largeBufferIndex;

                AllocateLargeBuffer();
                return largeBufferIndex;
            }
        }

        private void AllocateLargeBuffer()
        {
            var largeBuffer = new LargeBuffer(_largeBufferSize);
            _pinnedBuffers.Add(largeBuffer);
        }

        private int GetNextSegmentIndex()
        {
            while (true)
            {
                var segmentIndex = Volatile.Read(ref _segmentIndex);
                if (Interlocked.CompareExchange(ref _segmentIndex, segmentIndex + 1, segmentIndex) == segmentIndex)
                    return segmentIndex + 1;
            }
        }

        public void Dispose()
        {
            foreach (var pinnedBuffer in _pinnedBuffers)
            {
                pinnedBuffer.Dispose();
            }
        }

        private unsafe class LargeBuffer : IDisposable
        {
            private GCHandle _handle;
            private readonly byte[] _buffer;
            private readonly byte* _bufferPointer;

            public LargeBuffer(int size)
            {
                _buffer = new byte[size];
                _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                _bufferPointer = (byte*)_handle.AddrOfPinnedObject().ToPointer();
            }

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
