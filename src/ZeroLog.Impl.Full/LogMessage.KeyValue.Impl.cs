using System;
using ZeroLog.Utils;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private partial void InternalAppendKeyValue(string key, string? value)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _stringIndex + 1 < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

            if (value is not null)
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
                *(ArgumentType*)_dataPointer = ArgumentType.Null;
                _dataPointer += sizeof(ArgumentType);
            }
        }
        else
        {
            TruncateMessage();
        }
    }

    private partial void InternalAppendKeyValue<T>(string key, T value, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

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

    private partial void InternalAppendKeyValue<T>(string key, T? value, ArgumentType argType)
        where T : unmanaged
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

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
            TruncateMessage();
        }
    }

    private partial void InternalAppendKeyValue<T>(string key, T value)
        where T : struct, Enum
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(EnumArg) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

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

    private partial void InternalAppendKeyValue<T>(string key, T? value)
        where T : struct, Enum
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(EnumArg) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

            if (value is not null)
            {
                *(ArgumentType*)_dataPointer = ArgumentType.Enum;
                _dataPointer += sizeof(ArgumentType);

                *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value.GetValueOrDefault()));
                _dataPointer += sizeof(EnumArg);
            }
            else
            {
                *(ArgumentType*)_dataPointer = ArgumentType.Null;
                _dataPointer += sizeof(ArgumentType);
            }
        }
        else
        {
            TruncateMessage();
        }
    }

    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<char> value)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + value.Length <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

            AppendAsciiString(value);
        }
        else
        {
            TruncateMessage();
        }
    }

    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<byte> value)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(ArgumentType) + sizeof(int) + value.Length <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.KeyString;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = key;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;

            AppendAsciiString(value);
        }
        else
        {
            TruncateMessage();
        }
    }
}
