using System;
using ZeroLog.Support;

namespace ZeroLog;

unsafe partial class LogMessage
{
    internal partial void InternalAppendChain(InternalApi.Chain chain)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _objectIndex < _objects.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Chain;
            _dataPointer += sizeof(ArgumentType);

            _objects[_objectIndex] = chain;

            *_dataPointer = _objectIndex;
            ++_dataPointer;

            ++_objectIndex;
        }
        else
        {
            TruncateMessage();
        }
    }

    internal partial void InternalAppendChainValue(string? value)
    {
        if (_isTruncated)
            return;

        if (_dataPointer + sizeof(byte) <= _endOfBuffer)
        {
            if (value is not null)
            {
                if (_objectIndex < _objects.Length)
                {
                    _objects[_objectIndex] = value;

                    *_dataPointer = _objectIndex;
                    ++_dataPointer;

                    ++_objectIndex;
                }
                else
                {
                    _isTruncated = true;
                }
            }
            else
            {
                *_dataPointer = InternalApi.Chain.NullStringIndex;
                ++_dataPointer;
            }
        }
        else
        {
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainValue<T>(T value)
        where T : unmanaged
    {
        if (_isTruncated)
            return;

        if (_dataPointer + sizeof(T) <= _endOfBuffer)
        {
            *(T*)_dataPointer = value;
            _dataPointer += sizeof(T);
        }
        else
        {
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainValue<T>(T? value)
        where T : unmanaged
    {
        if (_isTruncated)
            return;

        if (_dataPointer + 1 + sizeof(T) <= _endOfBuffer)
        {
            if (value is not null)
            {
                *_dataPointer = 1;
                ++_dataPointer;

                *(T*)_dataPointer = value.GetValueOrDefault();
                _dataPointer += sizeof(T);
            }
            else
            {
                *_dataPointer = 0;
                ++_dataPointer;
            }
        }
        else
        {
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainEnumValue<T>(T value)
        where T : struct, Enum
    {
        if (_isTruncated)
            return;

        if (_dataPointer + sizeof(EnumArg) <= _endOfBuffer)
        {
            *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value));
            _dataPointer += sizeof(EnumArg);
        }
        else
        {
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainEnumValue<T>(T? value)
        where T : struct, Enum
    {
        if (_isTruncated)
            return;

        if (_dataPointer + 1 + sizeof(EnumArg) <= _endOfBuffer)
        {
            if (value is not null)
            {
                *_dataPointer = 1;
                ++_dataPointer;

                *(EnumArg*)_dataPointer = new EnumArg(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value.GetValueOrDefault()));
                _dataPointer += sizeof(EnumArg);
            }
            else
            {
                *_dataPointer = 0;
                ++_dataPointer;
            }
        }
        else
        {
            _isTruncated = true;
        }
    }
}
