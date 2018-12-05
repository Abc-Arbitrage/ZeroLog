using System;
using System.Text.Formatting;

namespace ZeroLog
{
    internal unsafe struct ArgSet : IArgSet
    {
        private readonly IntPtr[] _argPointers;
        private readonly string[] _strings;

        public ArgSet(IntPtr[] argPointers, string[] strings, int argCount)
        {
            _argPointers = argPointers;
            _strings = strings;
            BytesRead = 0;
            Count = argCount - 1;
        }

        public int Count { get; }

        public int BytesRead { get; private set; }

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var argPointer = (byte*)_argPointers[index + 1].ToPointer();

            var dataPointer = argPointer;
            stringBuffer.Append(ref dataPointer, format, _strings, _argPointers, Count + 1);

            BytesRead += (int)(dataPointer - argPointer);
        }
    }
}
