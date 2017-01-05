using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    public unsafe struct ArgSet : IArgSet
    {
        private readonly byte[] _buffer;
        private readonly List<int> _offsets;
        private readonly List<string> _strings;

        public ArgSet(byte[] buffer, List<int> offsets, List<string> strings)
        {
            _buffer = buffer;
            _offsets = offsets;
            _strings = strings;
        }

        public int Count => _offsets.Count;

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var offset = _offsets[index];
            stringBuffer.AppendFrom(_buffer, offset, format, _strings);
        }
    }
}