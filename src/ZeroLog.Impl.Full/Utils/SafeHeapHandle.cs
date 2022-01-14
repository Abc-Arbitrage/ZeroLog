using System;
using System.Runtime.InteropServices;

namespace ZeroLog.Utils
{
    internal sealed class SafeHeapHandle : SafeHandle
    {
        public int ByteLength { get; }

        public override bool IsInvalid => handle == IntPtr.Zero;

        public SafeHeapHandle(int byteLength)
            : base(IntPtr.Zero, true)
        {
            ByteLength = byteLength;

            handle = Marshal.AllocHGlobal(byteLength);

            if (handle == IntPtr.Zero)
                throw new OutOfMemoryException();
        }

        protected override bool ReleaseHandle()
        {
            var handleCopy = handle;
            handle = IntPtr.Zero;

            if (handleCopy != IntPtr.Zero)
                Marshal.FreeHGlobal(handleCopy);

            return true;
        }
    }
}
