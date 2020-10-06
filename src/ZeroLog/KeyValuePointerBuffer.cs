using System;
using System.Collections.Generic;

namespace ZeroLog
{
    internal class KeyValuePointerBuffer
    {
        private readonly List<IntPtr> _keyValuePointers = new List<IntPtr>(byte.MaxValue);

        public int PointerCount => _keyValuePointers.Count;

        public unsafe byte* GetUnsafePointer(int index)
        {
            return (byte*)_keyValuePointers[index].ToPointer();
        }

        public unsafe void AddUnsafePointer(byte* pointer)
        {
            _keyValuePointers.Add(new IntPtr(pointer));
        }

        public void Clear()
        {
            _keyValuePointers.Clear();
        }
    }
}
