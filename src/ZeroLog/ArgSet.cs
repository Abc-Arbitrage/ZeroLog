using System;
using System.Text.Formatting;

namespace ZeroLog
{
    internal unsafe struct ArgSet : IArgSet
    {
        private readonly IntPtr[] _argPointers;
        private readonly string[] _strings;
        private readonly int _argOffset;
        private readonly int _totalArgCount;

        public ArgSet(IntPtr[] argPointers, string[] strings, int argOffset, int totalArgCount)
        {
            _argPointers = argPointers;
            _strings = strings;
            _argOffset = argOffset;
            _totalArgCount = totalArgCount;
            BytesRead = 0;
        }

        public int Count => _totalArgCount - _argOffset;

        public int BytesRead { get; private set; }

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var argPointer = (byte*)_argPointers[index + _argOffset].ToPointer();

            var dataPointer = argPointer;
            stringBuffer.Append(ref dataPointer, format, _strings, _argPointers, _totalArgCount);

            BytesRead += (int)(dataPointer - argPointer);
        }
    }
}
