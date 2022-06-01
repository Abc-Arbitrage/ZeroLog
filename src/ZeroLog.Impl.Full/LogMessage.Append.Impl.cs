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
    internal partial void InternalAppendStringSpan(ReadOnlySpan<char> value)
    {
        if (value.Length > 0)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer) - sizeof(ArgumentType) - sizeof(int);

            if (remainingBytes > 0)
            {
                var lengthInChars = value.Length;

                if (lengthInChars * sizeof(char) > remainingBytes)
                {
                    lengthInChars = remainingBytes / sizeof(char);

                    if (lengthInChars == 0)
                    {
                        TruncateMessage();
                        return;
                    }

                    _isTruncated = true;
                }

                *(ArgumentType*)_dataPointer = ArgumentType.StringSpan;
                _dataPointer += sizeof(ArgumentType);

                *(int*)_dataPointer = lengthInChars;
                _dataPointer += sizeof(int);

                value.Slice(0, lengthInChars).CopyTo(new Span<char>(_dataPointer, lengthInChars));
                _dataPointer += lengthInChars * sizeof(char);
            }
            else
            {
                TruncateMessage();
            }
        }
    }

    [SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
    internal partial void InternalAppendUtf8StringSpan(ReadOnlySpan<byte> value)
    {
        if (value.Length > 0)
        {
            var remainingBytes = (int)(_endOfBuffer - _dataPointer) - sizeof(ArgumentType) - sizeof(int);

            if (remainingBytes > 0)
            {
                var lengthInBytes = value.Length;

                if (lengthInBytes > remainingBytes)
                {
                    _isTruncated = true;
                    lengthInBytes = remainingBytes;
                }

                *(ArgumentType*)_dataPointer = ArgumentType.Utf8StringSpan;
                _dataPointer += sizeof(ArgumentType);

                *(int*)_dataPointer = lengthInBytes;
                _dataPointer += sizeof(int);

                value.Slice(0, lengthInBytes).CopyTo(new Span<byte>(_dataPointer, lengthInBytes));
                _dataPointer += lengthInBytes;
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
