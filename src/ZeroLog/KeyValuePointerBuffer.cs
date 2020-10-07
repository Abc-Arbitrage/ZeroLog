using System;
using System.Collections.Generic;

namespace ZeroLog
{
    internal class KeyValuePointerBuffer
    {
        private readonly List<IntPtr> _keyPointers = new List<IntPtr>(byte.MaxValue);

        public int KeyPointerCount => _keyPointers.Count;

        public unsafe byte* GetKeyPointer(int index)
            => (byte*)_keyPointers[index].ToPointer();

        public unsafe void AddKeyPointer(byte* pointer)
            => _keyPointers.Add(new IntPtr(pointer));

        public void Clear()
            => _keyPointers.Clear();
    }
}
