using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Formatting;
using ZeroLog.Utils;

namespace ZeroLog
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
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

        public void AppendTo(StringBuffer stringBuffer, byte* valuePtr)
        {
            if (!UnmanagedCache.TryGetFormatter(_typeHandle, out var formatter))
            {
                AppendUnregistered(stringBuffer, valuePtr, _typeSize);
                return;
            }

            formatter(stringBuffer, valuePtr, StringView.Empty);
        }

        private static void AppendUnregistered(StringBuffer buffer, byte* valuePtr, int size)
        {
            buffer.Append("Unmanaged(0x");
            HexUtils.AppendValueAsHex(buffer, valuePtr, size);
            buffer.Append(")");
        }
    }
}
