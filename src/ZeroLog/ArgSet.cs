using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    internal unsafe struct ArgSet : IArgSet
    {
        private readonly List<IntPtr> _argPointers;
        private readonly List<string> _strings;

        public ArgSet(List<IntPtr> argPointers, List<string> strings)
        {
            _argPointers = argPointers;
            _strings = strings;
            BytesRead = 0;
        }

        public int Count => _argPointers.Count - 1;

        public int BytesRead { get; private set; }

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var argPointer = (byte*)_argPointers[index + 1].ToPointer();

            var dataPointer = argPointer;
            stringBuffer.Append(ref dataPointer, format, _strings, _argPointers);

            BytesRead += (int)(dataPointer - argPointer);
        }
    }
}
