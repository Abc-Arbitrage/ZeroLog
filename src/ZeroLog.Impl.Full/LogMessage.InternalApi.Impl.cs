namespace ZeroLog;

unsafe partial class LogMessage
{
    internal partial void InternalAppendChain(string chain)
    {
        if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            *(ArgumentType*)_dataPointer = ArgumentType.Chain;
            _dataPointer += sizeof(ArgumentType);

            _strings[_stringIndex] = chain;

            *_dataPointer = _stringIndex;
            ++_dataPointer;

            ++_stringIndex;
        }
        else
        {
            _isTruncated = true; // TODO: _truncatedAt
        }
    }

    internal partial void InternalAppendChainLinkValue(string? value)
    {
        if (_dataPointer + sizeof(byte) <= _endOfBuffer && _stringIndex < _strings.Length)
        {
            if (value is not null)
            {
                _strings[_stringIndex] = value;

                *_dataPointer = _stringIndex;
                ++_dataPointer;

                ++_stringIndex;
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

    internal partial void InternalAppendChainLinkValue<T>(T value)
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

    internal partial void InternalAppendChainLinkValue<T>(T? value)
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
