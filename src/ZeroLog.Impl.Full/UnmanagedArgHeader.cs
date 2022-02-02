using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;
using ZeroLog.Utils;

namespace ZeroLog;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
internal unsafe struct UnmanagedArgHeader
{
    private IntPtr _typeHandle;
    private int _typeSize;

    public int Size => _typeSize;

    public UnmanagedArgHeader(IntPtr typeHandle, int typeSize)
    {
        _typeHandle = typeHandle;
        _typeSize = typeSize;
    }

    public bool TryAppendTo(byte* valuePtr, Span<char> destination, out int charsWritten, string? format, ZeroLogConfiguration config)
    {
        if (UnmanagedCache.TryGetFormatter(_typeHandle, out var formatter))
            return formatter.Invoke(valuePtr, destination, out charsWritten, format, config);

        return TryAppendUnformattedTo(valuePtr, destination, out charsWritten);
    }

    public bool TryAppendUnformattedTo(byte* valuePtr, Span<char> destination, out int charsWritten)
    {
        const string prefix = "Unmanaged(0x";
        const string suffix = ")";

        var outputSize = prefix.Length + suffix.Length + 2 * _typeSize;

        if (destination.Length < outputSize)
        {
            charsWritten = 0;
            return false;
        }

        prefix.CopyTo(destination);
        HexUtils.AppendValueAsHex(valuePtr, _typeSize, destination.Slice(prefix.Length));
        suffix.CopyTo(destination.Slice(prefix.Length + 2 * _typeSize));

        charsWritten = outputSize;
        return true;
    }
}
