using System;
using System.Collections.Generic;

namespace ZeroLog.Formatting;

internal class KeyValuePointerBuffer
{
    private readonly List<IntPtr> _keyPointers = new(byte.MaxValue);

    public string?[] Strings = Array.Empty<string?>();

    public int KeyPointerCount => _keyPointers.Count;

    public void Init(string?[] strings)
    {
        Strings = strings;
        _keyPointers.Clear();
    }

    public unsafe byte* GetKeyPointer(int index)
        => (byte*)_keyPointers[index].ToPointer();

    public unsafe void AddKeyPointer(byte* pointer)
        => _keyPointers.Add(new IntPtr(pointer));
}
