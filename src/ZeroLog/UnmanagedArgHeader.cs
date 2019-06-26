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

        public void AppendTo(StringBuffer stringBuffer, byte* valuePtr, StringView format)
        {
            if (!UnmanagedCache.TryGetFormatter(_typeHandle, out var formatter))
            {
                AppendUnformattedTo(stringBuffer, valuePtr);
                return;
            }

            formatter(stringBuffer, valuePtr, format);
        }

        public void AppendUnformattedTo(StringBuffer stringBuffer, byte* valuePtr)
        {
            stringBuffer.Append("Unmanaged(0x");
            HexUtils.AppendValueAsHex(stringBuffer, valuePtr, _typeSize);
            stringBuffer.Append(")");
        }
    }
}
