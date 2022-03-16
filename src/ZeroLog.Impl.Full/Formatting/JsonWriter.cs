using System;
using System.Runtime.CompilerServices;

namespace ZeroLog.Formatting;

internal static unsafe class JsonWriter
{
    public static void WriteJsonToStringBuffer(KeyValueList keyValueList, Span<char> destination, out int charsWritten)
    {
        var builder = new CharBufferBuilder(destination);

        builder.Append('{');
        builder.Append(' ');

        var first = true;

        foreach (var keyValue in keyValueList)
        {
            if (!first)
            {
                builder.Append(',');
                builder.Append(' ');
            }

            AppendString(ref builder, keyValue.Key);

            builder.Append(':');
            builder.Append(' ');

            AppendJsonValue(ref builder, keyValue);

            first = false;
        }

        builder.Append(' ');
        builder.Append('}');

        charsWritten = builder.Length;
    }

    private static void AppendJsonValue(ref CharBufferBuilder builder, in FormattedKeyValue keyValue)
    {
        if (keyValue.IsBoolean)
            builder.TryAppendWhole(keyValue.Value.SequenceEqual(bool.TrueString) ? "true" : "false");
        else if (keyValue.IsNumeric)
            builder.TryAppendWhole(keyValue.Value);
        else if (keyValue.IsNull)
            builder.TryAppendWhole("null");
        else
            AppendString(ref builder, keyValue.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendString(ref CharBufferBuilder builder, ReadOnlySpan<char> value)
    {
        builder.Append('"');

        foreach (var c in value)
            AppendEscapedChar(c, ref builder);

        builder.Append('"');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendEscapedChar(char c, ref CharBufferBuilder builder)
    {
        // Escape characters based on https://tools.ietf.org/html/rfc7159

        if (c is '\\' or '"' or <= '\u001F')
            AppendControlChar(c, ref builder);
        else
            builder.Append(c);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AppendControlChar(char c, ref CharBufferBuilder builder)
    {
        switch (c)
        {
            case '"':
                builder.TryAppendWhole(@"\""");
                break;

            case '\\':
                builder.TryAppendWhole(@"\\");
                break;

            case '\b':
                builder.TryAppendWhole(@"\b");
                break;

            case '\t':
                builder.TryAppendWhole(@"\t");
                break;

            case '\n':
                builder.TryAppendWhole(@"\n");
                break;

            case '\f':
                builder.TryAppendWhole(@"\f");
                break;

            case '\r':
                builder.TryAppendWhole(@"\r");
                break;

            default:
            {
                const string prefix = @"\u00";
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
