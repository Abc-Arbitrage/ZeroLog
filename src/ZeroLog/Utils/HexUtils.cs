using System.Text.Formatting;

namespace ZeroLog.Utils
{
    internal static class HexUtils
    {
        public static unsafe void AppendValueAsHex(StringBuffer buffer, byte* valuePtr, int size)
        {
            for (int index = 0; index < size; ++index)
            {
                var char0Index = valuePtr[index] & 0xf;
                var char1Index = (valuePtr[index] & 0xf0) >> 4;

                buffer.Append(_hexTable[char1Index]);
                buffer.Append(_hexTable[char0Index]);
            }
        }

        private static readonly char[] _hexTable = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
    }
}
