using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Utils;

namespace ZeroLog;

public sealed unsafe partial class LogMessage
{
    internal static LogMessage Empty { get; } = new(string.Empty);

    private readonly byte* _startOfBuffer;
    private readonly byte* _endOfBuffer;
    private readonly string?[] _strings;

    private byte* _dataPointer;
    private byte _stringIndex;
    private bool _isTruncated;

    public Level Level { get; private set; }
    public DateTime Timestamp { get; internal set; }
    public Thread? Thread { get; private set; }

    internal Log? Logger { get; private set; }
    internal bool IsTruncated => _isTruncated;

    internal string? ConstantMessage { get; }
    internal bool IsPooled => ConstantMessage is null;

    internal LogMessage(string message)
    {
        ConstantMessage = message;
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
        Logger = log;

        _dataPointer = _startOfBuffer;
        _stringIndex = 0;
        _isTruncated = false;
    }

    public void Log()
    {
        if (!ReferenceEquals(this, Empty))
            Logger?.Enqueue(this);
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
    internal LogMessage Append(string? value)
    {
        // TODO remove this method

        InternalAppendString(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage AppendEnum<T>(T value)
        where T : struct, Enum
    {
        InternalAppendEnum(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public LogMessage AppendEnum<T>(T? value)
        where T : struct, Enum
    {
        InternalAppendEnum(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InternalAppendString(string? value)
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

    private void InternalAppendValueType<T>(T value, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(T) <= _endOfBuffer)
        {
            *(ArgumentType*)_dataPointer = argType;
            _dataPointer += sizeof(ArgumentType);

            *(T*)_dataPointer = value;
            _dataPointer += sizeof(T);
        }
        else
        {
            _isTruncated = true;
        }
    }

    private void InternalAppendValueType<T>(T? value, ArgumentType argType)
        where T : unmanaged
    {
        if (value is not null)
            InternalAppendValueType(value.GetValueOrDefault(), argType);
        else
            InternalAppendNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InternalAppendValueType<T>(T value, string format, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = argType | ArgumentType.FormatFlag;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = format;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

            *(T*)_dataPointer = value;
            _dataPointer += sizeof(T);
        }
        else
        {
            _isTruncated = true;
        }
    }

    private void InternalAppendValueType<T>(T? value, string format, ArgumentType argType)
        where T : unmanaged
    {
        if (value is not null)
            InternalAppendValueType(value.GetValueOrDefault(), format, argType);
        else
            InternalAppendNull();
    }

    private void InternalAppendEnum<T>(T value)
        where T : struct, Enum
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(EnumArg) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Enum;
            _dataPointer += sizeof(ArgumentType);

            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
        }
        else
        {
            _isTruncated = true;
        }
    }

    private void InternalAppendEnum<T>(T? value)
        where T : struct, Enum
    {
        if (value is not null)
            InternalAppendEnum(value.GetValueOrDefault());
        else
            InternalAppendNull();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InternalAppendKeyValue<T>(string key, T value, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_stringIndex;

            *(ArgumentType*)_dataPointer = argType;
            _dataPointer += sizeof(ArgumentType);

            *(T*)_dataPointer = value;
            _dataPointer += sizeof(T);
        }
        else
        {
            _isTruncated = true;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InternalAppendKeyValue<T>(string key, T? value, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_stringIndex;

            if (value is not null)
            {
                *(ArgumentType*)_dataPointer = argType;
                _dataPointer += sizeof(ArgumentType);

                *(T*)_dataPointer = value.GetValueOrDefault();
                _dataPointer += sizeof(T);
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
    }
}
