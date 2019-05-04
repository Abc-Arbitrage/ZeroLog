using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Formatting;

namespace ZeroLog.Utils
{
    internal static class HexUtils
    {
        public static unsafe void AppendValueAsHex(StringBuffer buffer, byte* value_ptr, int size)
        {
            for (int ii = 0; ii < size; ++ii)
            {
                var char0_index = value_ptr[ii] & 0xf;
                var char1_index = (value_ptr[ii] & 0xf0) >> 4;

                buffer.Append(hex_table[char1_index]);
                buffer.Append(hex_table[char0_index]);
            }
        }

        private static readonly char[] hex_table = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
    }
}
