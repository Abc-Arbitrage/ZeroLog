using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ZeroLog.Support;

namespace ZeroLog;

unsafe partial class LogMessage
{
    internal partial void InternalAppendString(string? value)
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
                TruncateMessage();
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
            TruncateMessage();
        }
    }

    internal partial void InternalAppendValueType<T>(T value, ArgumentType argType)
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
            TruncateMessage();
        }
    }

    internal partial void InternalAppendValueType<T>(T? value, ArgumentType argType)
        where T : unmanaged
    {
        if (value is not null)
            InternalAppendValueType(value.GetValueOrDefault(), argType);
        else
            InternalAppendNull();
    }

    internal partial void InternalAppendValueType<T>(T value, string format, ArgumentType argType)
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
            TruncateMessage();
        }
    }

    internal partial void InternalAppendValueType<T>(T? value, string format, ArgumentType argType)
        where T : unmanaged
    {
        if (value is not null)
            InternalAppendValueType(value.GetValueOrDefault(), format, argType);
        else
            InternalAppendNull();
    }

    internal partial void InternalAppendEnum<T>(T value)
        where T : struct, Enum
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(EnumArg) <= _endOfBuffer)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Enum;
            _dataPointer += sizeof(ArgumentType);

            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendEnum<T>(T? value)
        where T : struct, Enum
    {
        if (value is not null)
            InternalAppendEnum(value.GetValueOrDefault());
        else
            InternalAppendNull();
    }

    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    private partial void InternalAppendAsciiString(ReadOnlySpan<char> value)
    {
        if (value.Length > 0)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer) - sizeof(ArgumentType) - sizeof(int);

            if (remainingBytes > 0)
            {
                var length = value.Length;

                if (length > remainingBytes)
                {
                    _isTruncated = true;
                    length = remainingBytes;
                }

                *(ArgumentType*)_dataPointer = ArgumentType.AsciiString;
                _dataPointer += sizeof(ArgumentType);

                *(int*)_dataPointer = length;
                _dataPointer += sizeof(int);

                foreach (var c in value.Slice(0, length))
                    *_dataPointer++ = (byte)c;
            }
            else
            {
                TruncateMessage();
            }
        }
    }

    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    private partial void InternalAppendAsciiString(ReadOnlySpan<byte> value)
    {
        if (value.Length > 0)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer) - sizeof(ArgumentType) - sizeof(int);

            if (remainingBytes > 0)
            {
                var length = value.Length;

                if (length > remainingBytes)
                {
                    _isTruncated = true;
                    length = remainingBytes;
                }

                *(ArgumentType*)_dataPointer = ArgumentType.AsciiString;
                _dataPointer += sizeof(ArgumentType);

                *(int*)_dataPointer = length;
                _dataPointer += sizeof(int);

                value.Slice(0, length).CopyTo(new Span<byte>(_dataPointer, length));
                _dataPointer += length;
            }
            else
            {
                TruncateMessage();
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void TruncateMessage()
    {
        if (_isTruncated)
            return;

        _isTruncated = true;

        if (_dataPointer + sizeof(ArgumentType) <= _endOfBuffer)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.EndOfTruncatedMessage;
            _dataPointer += sizeof(ArgumentType);
        }
    }
}
