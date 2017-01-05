using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    public unsafe struct ArgSet : IArgSet
    {
        private readonly byte[] _buffer;
        private readonly List<string> _strings;
        private readonly List<int> _offsets;

        public ArgSet(byte[] buffer, List<string> strings, List<int> offsets)
        {
            _buffer = buffer;
            _strings = strings;
            _offsets = offsets;
        }

        public int Count => _offsets.Count - 1;

        public void Format(StringBuffer stringBuffer, int index, StringView format)
        {
            var offset = _offsets[index + 1]; //_offsets[0] is format string's offset
            stringBuffer.AppendFrom(_buffer, offset, format, _strings, _offsets);
        }
    }
}