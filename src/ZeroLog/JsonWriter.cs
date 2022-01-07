using System;
using System.Runtime.CompilerServices;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal static unsafe class JsonWriter
    {
        public static void WriteJsonToStringBuffer(ref CharBufferBuilder builder, KeyValuePointerBuffer keyValuePointerBuffer)
        {
            builder.TryAppendWhole("{ ");

            for (var i = 0; i < keyValuePointerBuffer.KeyPointerCount; i++)
            {
                if (i != 0)
                    builder.TryAppendWhole(", ");

                var dataPointer = keyValuePointerBuffer.GetKeyPointer(i);

                AppendJsonKey(ref builder, keyValuePointerBuffer.Strings, ref dataPointer);

                builder.TryAppendWhole(": ");

                AppendJsonValue(ref builder, keyValuePointerBuffer.Strings, ref dataPointer);
            }

            builder.TryAppendWhole(" }");
        }

        private static void AppendJsonKey(ref CharBufferBuilder builder, string?[] strings, ref byte* dataPointer)
        {
            ++dataPointer; // ArgType
            var keyIndex = *dataPointer;
            ++dataPointer; // Key index

            var key = strings[keyIndex] ?? string.Empty;
            AppendString(key, ref builder);
        }

        private static void AppendJsonValue(ref CharBufferBuilder builder, string?[] strings, ref byte* dataPointer)
        {
            var argumentType = *(ArgumentType*)dataPointer;
            dataPointer += sizeof(ArgumentType);

            if ((argumentType & ArgumentType.FormatFlag) != 0)
            {
                argumentType &= ~ArgumentType.FormatFlag;
                ++dataPointer;
            }

            switch (argumentType)
            {
                case ArgumentType.KeyString:
                case ArgumentType.String:
                    AppendString(strings[*dataPointer], ref builder);
                    break;

                case ArgumentType.AsciiString:
                    var length = *(int*)dataPointer;
                    builder.TryAppend('"');

                    for (var i = 0; i < length; ++i)
                        AppendEscapedChar((char)*(dataPointer + i), ref builder);

                    builder.TryAppend('"');
                    break;

                case ArgumentType.Boolean:
                    builder.TryAppendWhole(*(bool*)dataPointer ? "true" : "false");
                    break;

                case ArgumentType.Byte:
                    builder.TryAppend(*dataPointer);
                    break;

                case ArgumentType.SByte:
                    builder.TryAppend(*(sbyte*)dataPointer);
                    break;

                case ArgumentType.Char:
                    builder.TryAppend('"');
                    AppendEscapedChar(*(char*)dataPointer, ref builder);
                    builder.TryAppend('"');
                    break;

                case ArgumentType.Int16:
                    builder.TryAppend(*(short*)dataPointer);
                    break;

                case ArgumentType.UInt16:
                    builder.TryAppend(*(ushort*)dataPointer);
                    break;

                case ArgumentType.Int32:
                    builder.TryAppend(*(int*)dataPointer);
                    break;

                case ArgumentType.UInt32:
                    builder.TryAppend(*(uint*)dataPointer);
                    break;

                case ArgumentType.Int64:
                    builder.TryAppend(*(long*)dataPointer);
                    break;

                case ArgumentType.UInt64:
                    builder.TryAppend(*(ulong*)dataPointer);
                    break;

                case ArgumentType.IntPtr:
                    builder.TryAppend(*(nint*)dataPointer);
                    break;

                case ArgumentType.UIntPtr:
                    builder.TryAppend(*(nuint*)dataPointer);
                    break;

                case ArgumentType.Single:
                    builder.TryAppend(*(float*)dataPointer);
                    break;

                case ArgumentType.Double:
                    builder.TryAppend(*(double*)dataPointer);
                    break;

                case ArgumentType.Decimal:
                    builder.TryAppend(*(decimal*)dataPointer);
                    break;

                case ArgumentType.Guid:
                    builder.TryAppend('"');
                    builder.TryAppend(*(Guid*)dataPointer);
                    builder.TryAppend('"');
                    break;

                case ArgumentType.DateTime:
                    builder.TryAppend('"');
                    builder.TryAppend(*(DateTime*)dataPointer, "yyyy-MM-dd HH:mm:ss");
                    builder.TryAppend('"');
                    break;

                case ArgumentType.TimeSpan:
                    builder.TryAppend('"');
                    builder.TryAppend(*(TimeSpan*)dataPointer, @"hh\:mm\:ss\.fffffff");
                    builder.TryAppend('"');
                    break;

                case ArgumentType.Enum:
                {
                    var enumArg = (EnumArg*)dataPointer;

                    var destination = builder.GetRemainingBuffer();
                    enumArg->TryFormat(destination, out var charsWritten);
                    builder.IncrementPos(charsWritten);

                    break;
                }

                case ArgumentType.Null:
                    builder.TryAppendWhole("null");
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendString(string? value, ref CharBufferBuilder builder)
        {
            builder.TryAppend('"');

            foreach (var c in value ?? string.Empty)
                AppendEscapedChar(c, ref builder);

            builder.TryAppend('"');
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendEscapedChar(char c, ref CharBufferBuilder builder)
        {
            // Escape characters based on https://tools.ietf.org/html/rfc7159

            if (c is '\\' or '"' or <= '\u001F')
                AppendControlChar(c, ref builder);
            else
                builder.TryAppend(c);
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
}
