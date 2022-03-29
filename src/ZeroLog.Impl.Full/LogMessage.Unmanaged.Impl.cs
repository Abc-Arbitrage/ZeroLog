using ZeroLog.Support;

namespace ZeroLog;

unsafe partial class LogMessage
{
    private partial void InternalAppendUnmanaged<T>(ref T value, string? format)
        where T : unmanaged
    {
        if (string.IsNullOrEmpty(format))
        {
            if (_dataPointer + sizeof(ArgumentType) + sizeof(UnmanagedArgHeader) + sizeof(T) <= _endOfBuffer)
            {
                *(ArgumentType*)_dataPointer = ArgumentType.Unmanaged;
                _dataPointer += sizeof(ArgumentType);

                *(UnmanagedArgHeader*)_dataPointer = new UnmanagedArgHeader(TypeUtil<T>.TypeHandle, sizeof(T));
                _dataPointer += sizeof(UnmanagedArgHeader);

                *(T*)_dataPointer = value;
                _dataPointer += sizeof(T);
            }
            else
            {
                TruncateMessage();
            }
        }
        else
        {
            if (_dataPointer + sizeof(ArgumentType) + sizeof(byte) + sizeof(UnmanagedArgHeader) + sizeof(T) <= _endOfBuffer && _stringIndex < _strings.Length)
            {
                *(ArgumentType*)_dataPointer = ArgumentType.Unmanaged | ArgumentType.FormatFlag;
                _dataPointer += sizeof(ArgumentType);

                _strings[_stringIndex] = format;

                *_dataPointer = _stringIndex;
                ++_dataPointer;

                ++_stringIndex;

                *(UnmanagedArgHeader*)_dataPointer = new UnmanagedArgHeader(TypeUtil<T>.TypeHandle, sizeof(T));
                _dataPointer += sizeof(UnmanagedArgHeader);

                *(T*)_dataPointer = value;
                _dataPointer += sizeof(T);
            }
            else
            {
                TruncateMessage();
            }
        }
    }

    private partial void InternalAppendUnmanaged<T>(ref T? value, string? format)
        where T : unmanaged
    {
        if (value != null)
        {
            var notNullValue = value.GetValueOrDefault();
            InternalAppendUnmanaged(ref notNullValue, format);
        }
        else
        {
            InternalAppendNull();
        }
    }
}
