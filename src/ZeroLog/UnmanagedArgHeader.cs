using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Formatting;
using ZeroLog.Utils;

namespace ZeroLog
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct UnmanagedArgHeader
    {
        private IntPtr _typeHandle;
        private int _typeSize;

        public int ArgumentSize => _typeSize;

        public UnmanagedArgHeader(IntPtr typeHandle, int typeSize)
        {
            _typeHandle = typeHandle;
            _typeSize = typeSize;
        }

        public void AppendTo(StringBuffer stringBuffer, byte* value_ptr)
        {
            if (!UnmanagedCache.TryGetFormatter(_typeHandle, out var formatter))
            {
                stringBuffer.Append("[unregistered unmanaged struct]");
                return;
            }
            formatter(stringBuffer, value_ptr, StringView.Empty);
        }
    }
}
