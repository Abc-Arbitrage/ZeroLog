using System;

namespace ZeroLog.Formatting;

internal static class HexUtils
{
    private static readonly char[] _hexTable = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'];

    public static unsafe void AppendValueAsHex(byte* valuePtr, int size, Span<char> destination)
    {
        for (var index = 0; index < size; ++index)
        {
            var char0Index = valuePtr[index] & 0xf;
            var char1Index = (valuePtr[index] & 0xf0) >> 4;

            destination[2 * index] = _hexTable[char1Index];
            destination[2 * index + 1] = _hexTable[char0Index];
        }
    }

    public static unsafe void AppendValueAsHex(byte* valuePtr, int size, Span<byte> destination)
    {
        var hexTableUtf8 = "0123456789abcdef"u8;

        for (var index = 0; index < size; ++index)
        {
            var char0Index = valuePtr[index] & 0xf;
            var char1Index = (valuePtr[index] & 0xf0) >> 4;

            destination[2 * index] = hexTableUtf8[char1Index];
            destination[2 * index + 1] = hexTableUtf8[char0Index];
        }
    }
}
