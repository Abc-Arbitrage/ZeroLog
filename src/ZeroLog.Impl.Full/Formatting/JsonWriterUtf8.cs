using System;
using System.Runtime.CompilerServices;

namespace ZeroLog.Formatting;

#if NET8_0_OR_GREATER

internal static unsafe class JsonWriterUtf8
{
    public static void WriteJsonToStringBuffer(KeyValueList keyValueList, Span<byte> destination, out int charsWritten)
    {
        var builder = new ByteBufferBuilder(destination);

        builder.AppendAscii('{');
        builder.AppendAscii(' ');

        var first = true;

        foreach (var keyValue in keyValueList)
        {
            if (!first)
            {
                builder.AppendAscii(',');
                builder.AppendAscii(' ');
            }

            AppendString(ref builder, keyValue.Key);

            builder.AppendAscii(':');
            builder.AppendAscii(' ');

            AppendJsonValue(ref builder, keyValue);

            first = false;
        }

        builder.AppendAscii(' ');
        builder.AppendAscii('}');

        charsWritten = builder.Length;
    }

    private static void AppendJsonValue(ref ByteBufferBuilder builder, in LoggedKeyValue keyValue)
    {
        if (keyValue.IsBoolean)
            builder.TryAppendWhole(keyValue.Value.SequenceEqual(bool.TrueString) ? "true"u8 : "false"u8);
        else if (keyValue.IsNumeric)
            builder.TryAppendWhole(keyValue.Value);
        else if (keyValue.IsNull)
            builder.TryAppendWhole("null"u8);
        else
            AppendString(ref builder, keyValue.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendString(ref ByteBufferBuilder builder, ReadOnlySpan<char> value)
    {
        builder.AppendAscii('"');

        foreach (var c in value)
            AppendEscapedChar(c, ref builder);

        builder.AppendAscii('"');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendEscapedChar(char c, ref ByteBufferBuilder builder)
    {
        // Escape characters based on https://tools.ietf.org/html/rfc7159

        if (c is '\\' or '"' or <= '\u001F')
            AppendControlChar(c, ref builder);
        else
            builder.TryAppendPartialChars(new ReadOnlySpan<char>(in c));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AppendControlChar(char c, ref ByteBufferBuilder builder)
    {
        switch (c)
        {
            case '"':
                builder.TryAppendWhole(@"\"""u8);
                break;

            case '\\':
                builder.TryAppendWhole(@"\\"u8);
                break;

            case '\b':
                builder.TryAppendWhole(@"\b"u8);
                break;

            case '\t':
                builder.TryAppendWhole(@"\t"u8);
                break;

            case '\n':
                builder.TryAppendWhole(@"\n"u8);
                break;

            case '\f':
                builder.TryAppendWhole(@"\f"u8);
                break;

            case '\r':
                builder.TryAppendWhole(@"\r"u8);
                break;

            default:
            {
                var prefix = @"\u00"u8;
                var destination = builder.GetRemainingBuffer();

                if (destination.Length >= prefix.Length + 2)
                {
                    builder.TryAppendWhole(prefix);

                    var byteValue = unchecked((byte)c);
                    HexUtils.AppendValueAsHex(&byteValue, 1, builder.GetRemainingBuffer());
                    builder.IncrementPos(2);
                }

                break;
            }
        }
    }
}

#endif
