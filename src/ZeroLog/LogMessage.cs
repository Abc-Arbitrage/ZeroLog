using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ZeroLog;

public sealed unsafe partial class LogMessage
{
    internal static LogMessage Empty { get; } = new();

    private readonly byte* _startOfBuffer;
    private readonly byte* _endOfBuffer;
    private readonly string?[] _strings;

    private byte* _dataPointer;
    private byte _stringIndex;
    private bool _isTruncated;
    private Log? _log;

    public Level Level { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Thread? Thread { get; private set; }

    private LogMessage()
    {
        _strings = Array.Empty<string>();
    }

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

        _dataPointer = _startOfBuffer;
        _stringIndex = 0;
        _isTruncated = false;
        _log = log;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage AppendKeyValue(string key, string? value)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _stringIndex + 1 < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_stringIndex;

            if (value is not null)
            {
                *(ArgumentType*)_dataPointer = ArgumentType.String;
                _dataPointer += sizeof(ArgumentType);

                _strings[_stringIndex] = value;

                *_dataPointer = _stringIndex;
                ++_stringIndex;
            }
            else
            {
                *(ArgumentType*)_dataPointer = ArgumentType.Null;
                _dataPointer += sizeof(ArgumentType);
            }
        }
        else
        {
            _isTruncated = true;
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InternalAppend(string? value)
    {
        if (value is not null)
        {
            if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _stringIndex < _strings.Length)
            {
                *(ArgumentType*)_dataPointer = ArgumentType.String;
                _dataPointer += sizeof(ArgumentType);

                _strings[_stringIndex] = value;

                *_dataPointer = _stringIndex;
                ++_dataPointer;

                ++_stringIndex;
            }
            else
            {
                _isTruncated = true;
            }
        }
        else
        {
            InternalAppendNull();
        }
    }

    private void InternalAppendNull()
    {
        if (_dataPointer + sizeof(ArgumentType) <= _endOfBuffer)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Null;
            _dataPointer += sizeof(ArgumentType);
        }
        else
        {
            _isTruncated = true;
        }
    }
}
