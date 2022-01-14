using System;
using System.Threading;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private readonly byte* _startOfBuffer;
    private readonly byte* _endOfBuffer;
    private readonly string?[] _strings;

    private byte* _dataPointer;
    private byte _stringIndex;
    private bool _isTruncated;

    internal Log? Logger { get; private set; }
    internal bool IsTruncated => _isTruncated;

    internal string? ConstantMessage { get; }
    internal bool IsPooled => ConstantMessage is null;

    internal LogMessage(BufferSegment bufferSegment, int stringCapacity)
    {
        stringCapacity = Math.Min(stringCapacity, byte.MaxValue);
        _strings = new string[stringCapacity];

        _startOfBuffer = bufferSegment.Data;
        _dataPointer = bufferSegment.Data;
        _endOfBuffer = bufferSegment.Data + bufferSegment.Length;
    }

    internal void Initialize(Log? log, Level level)
    {
        Timestamp = DateTime.UtcNow; // TODO clock in Log
        Level = level;
        Thread = Thread.CurrentThread;
        Exception = null;
        Logger = log;

        _dataPointer = _startOfBuffer;
        _stringIndex = 0;
        _isTruncated = false;
    }

    public partial void Log()
    {
        if (!ReferenceEquals(this, Empty))
            Logger?.Submit(this);
    }
}
