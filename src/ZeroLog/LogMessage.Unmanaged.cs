using System.Runtime.CompilerServices;
using ZeroLog.Utils;

namespace ZeroLog;

unsafe partial class LogMessage
{
    public LogMessage AppendUnmanaged<T>(T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    public LogMessage AppendUnmanaged<T>(T? value, string? format = null)
        where T : unmanaged
    {
        if (value != null)
            return AppendUnmanaged(value.GetValueOrDefault(), format);

        InternalAppendNull();
        return this;
    }

    public LogMessage AppendUnmanaged<T>(ref T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    public LogMessage AppendUnmanaged<T>(ref T? value, string? format = null)
        where T : unmanaged
    {
        if (value != null)
            return AppendUnmanaged(value.GetValueOrDefault(), format);

        InternalAppendNull();
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void InternalAppendUnmanaged<T>(ref T value, string? format)
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
}
