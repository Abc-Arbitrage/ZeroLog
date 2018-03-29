using System;
using System.Runtime.InteropServices;

namespace ZeroLog
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EnumArg
    {
        public IntPtr TypeHandle;
        public ulong Value;

        public EnumArg(IntPtr typeHandle, ulong value)
        {
            TypeHandle = typeHandle;
            Value = value;
        }
    }
}
