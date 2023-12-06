using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Support;

namespace ZeroLog;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
internal readonly unsafe struct UnmanagedArgHeader(IntPtr typeHandle, int typeSize)
{
    public Type? Type => TypeUtil.GetTypeFromHandle(typeHandle);
    public int Size => typeSize;

    public bool TryAppendTo(byte* valuePtr, Span<char> destination, out int charsWritten, string? format, ZeroLogConfiguration config)
    {
        if (UnmanagedCache.TryGetFormatter(typeHandle, out var formatter))
            return formatter.Invoke(valuePtr, destination, out charsWritten, format, config);

        return TryAppendUnformattedTo(valuePtr, destination, out charsWritten);
    }

    public bool TryAppendUnformattedTo(byte* valuePtr, Span<char> destination, out int charsWritten)
    {
        const string prefix = "Unmanaged(0x";
        const string suffix = ")";

        var outputSize = prefix.Length + suffix.Length + 2 * typeSize;

        if (destination.Length < outputSize)
        {
            charsWritten = 0;
            return false;
        }

        prefix.CopyTo(destination);
        HexUtils.AppendValueAsHex(valuePtr, typeSize, destination.Slice(prefix.Length));
        suffix.CopyTo(destination.Slice(prefix.Length + 2 * typeSize));

        charsWritten = outputSize;
        return true;
    }
}
