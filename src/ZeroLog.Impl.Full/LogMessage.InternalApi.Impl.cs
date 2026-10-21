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
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainItem(string? value)
    {
        if (_dataPointer + sizeof(byte) <= _endOfBuffer && _objectIndex < _objects.Length)
        {
            if (value is not null)
            {
                _objects[_objectIndex] = value;

                *_dataPointer = _objectIndex;
                ++_dataPointer;

                ++_objectIndex;
            }
            else
            {
                *_dataPointer = 0; // The chain occupies one slot, so 0 is safe to use to mean null
                ++_dataPointer;
            }
        }
        else
        {
            _isTruncated = true;
        }
    }

    internal partial void InternalAppendChainItem<T>(T value)
        where T : unmanaged
    {
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

    internal partial void InternalAppendChainItem<T>(T? value)
        where T : unmanaged
    {
        if (_dataPointer + 1 + sizeof(T) <= _endOfBuffer)
        {
            if (value.HasValue)
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
}
