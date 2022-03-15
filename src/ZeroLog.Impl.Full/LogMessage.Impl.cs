using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace ZeroLog;

unsafe partial class LogMessage
{
    internal static readonly LogMessage Empty = new(string.Empty);

    [SuppressMessage("ReSharper", "NotAccessedField.Local", Justification = "This field is a GC root for the underlying buffer")]
    private readonly byte[]? _underlyingBuffer;

    private readonly byte* _startOfBuffer;
    private readonly byte* _endOfBuffer;
    private readonly string?[] _strings;

    private byte* _dataPointer;
    private byte _stringIndex;
    private bool _isTruncated;

    internal Log? Logger { get; private set; }
    internal bool IsTruncated => _isTruncated;

    internal string? ConstantMessage { get; }
    internal bool IsPooled { get; }

    internal LogMessage(string message)
    {
        ConstantMessage = message;
        _strings = Array.Empty<string>();
    }

    internal LogMessage(BufferSegment bufferSegment, int stringCapacity, bool isPooled)
    {
        stringCapacity = Math.Min(stringCapacity, byte.MaxValue);
        _strings = new string[stringCapacity];

        _startOfBuffer = bufferSegment.Data;
        _dataPointer = bufferSegment.Data;
        _endOfBuffer = bufferSegment.Data + bufferSegment.Length;
        _underlyingBuffer = bufferSegment.UnderlyingBuffer;

        IsPooled = isPooled;
    }

    internal void Initialize(Log? log, LogLevel level)
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

    public static LogMessage CreateTestMessage(LogLevel level, int bufferSize, int stringCapacity)
    {
        var message = new LogMessage(BufferSegmentProvider.CreateStandaloneSegment(bufferSize), stringCapacity, false);
        message.Initialize(null, level);
        return message;
    }
}
