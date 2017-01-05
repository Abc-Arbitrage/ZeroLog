using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    public unsafe struct ArgSet : IArgSet
    {
        private readonly List<IntPtr> _argPointers;
        private readonly List<string> _strings;

        public ArgSet(List<IntPtr> argPointers, List<string> strings)
        {
            _argPointers = argPointers;
            _strings = strings;
        }

        public int Count => _argPointers.Count - 1;

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var dataPointer = (byte*)_argPointers[index + 1].ToPointer();
            stringBuffer.AppendFrom(dataPointer, format, _strings, _argPointers);
        }
    }
}
