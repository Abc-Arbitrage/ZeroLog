using System;

namespace ZeroLog;

internal unsafe class BufferSegmentProvider
{
    private readonly object _lock = new();

    private readonly int _segmentCount;
    private readonly int _segmentSize;

    private byte[]? _currentBuffer;
    private int _currentSegment;

    internal int BufferSize => _segmentCount * _segmentSize;

    public BufferSegmentProvider(int segmentCount, int segmentSize)
    {
        if (segmentCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(segmentCount), "Invalid pool size");

        if (segmentSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(segmentSize), "Invalid buffer size");

        const int maxBufferSize = 1024 * 1024 * 1024;
        segmentSize = Math.Min(segmentSize, maxBufferSize);

        while ((long)segmentSize * segmentCount > maxBufferSize)
            segmentCount >>= 1;

        _segmentCount = segmentCount;
        _segmentSize = segmentSize;
    }

    public BufferSegment GetSegment()
    {
        lock (_lock)
        {
            if (_currentSegment >= _segmentCount || _currentBuffer is null)
            {
                _currentBuffer = GC.AllocateUninitializedArray<byte>(BufferSize, pinned: true);
                _currentSegment = 0;
            }

            fixed (byte* data = &_currentBuffer[_segmentSize * _currentSegment++])
            {
                return new BufferSegment(data, _segmentSize, _currentBuffer);
            }
        }
    }

    public static BufferSegment CreateStandaloneSegment(int bufferSize)
    {
        var buffer = GC.AllocateUninitializedArray<byte>(bufferSize, pinned: true);

        fixed (byte* data = buffer)
        {
            return new BufferSegment(data, bufferSize, buffer);
        }
    }
}

internal unsafe struct BufferSegment
{
    public readonly byte* Data;
    public readonly int Length;
    public readonly byte[]? UnderlyingBuffer;

    public BufferSegment(byte* data, int length, byte[]? underlyingBuffer)
    {
        Data = data;
        Length = length;
        UnderlyingBuffer = underlyingBuffer;
    }
}
