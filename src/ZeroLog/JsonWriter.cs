using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Formatting;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal static unsafe class JsonWriter
    {
        public static void WriteJsonToStringBuffer(StringBuffer stringBuffer, KeyValuePointerBuffer keyValuePointerBuffer, string[] strings)
        {
            stringBuffer.Append(LogManager.Config.JsonSeparator);
            stringBuffer.Append("{ ");

            for (var i = 0; i < keyValuePointerBuffer.KeyPointerCount; i++)
            {
                if (i != 0)
                    stringBuffer.Append(", ");

                var dataPointer = keyValuePointerBuffer.GetKeyPointer(i);

                // Key.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);

                stringBuffer.Append(": ");

                // Value.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);
            }

            stringBuffer.Append(" }");
        }

        private static void AppendJsonValue(StringBuffer stringBuffer, IReadOnlyList<string> strings, ref byte* dataPointer)
        {
            var argumentType = (ArgumentType)(*dataPointer & ArgumentTypeMask.ArgumentType);
            dataPointer += sizeof(ArgumentType);

            switch (argumentType)
            {
                case ArgumentType.KeyString:
                case ArgumentType.String:
                    stringBuffer.Append('"');

                    foreach (var c in strings[*dataPointer])
                        AppendEscapedChar(c, stringBuffer);

                    stringBuffer.Append('"');
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.Boolean:
                    stringBuffer.Append(*(bool*)dataPointer ? "true" : "false");
                    dataPointer += sizeof(bool);
                    break;

                case ArgumentType.Byte:
                    stringBuffer.Append(*dataPointer, StringView.Empty);
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.Char:
                    stringBuffer.Append('"');
                    AppendEscapedChar(*(char*)dataPointer, stringBuffer);
                    stringBuffer.Append('"');

                    dataPointer += sizeof(char);
                    break;

                case ArgumentType.Int16:
                    stringBuffer.Append(*(short*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(short);
                    break;

                case ArgumentType.Int32:
                    stringBuffer.Append(*(int*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(int);
                    break;

                case ArgumentType.Int64:
                    stringBuffer.Append(*(long*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(long);
                    break;

                case ArgumentType.Single:
                    stringBuffer.Append(*(float*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(float);
                    break;

                case ArgumentType.Double:
                    stringBuffer.Append(*(double*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(double);
                    break;

                case ArgumentType.Decimal:
                    stringBuffer.Append(*(decimal*)dataPointer, StringView.Empty);
                    dataPointer += sizeof(decimal);
                    break;

                case ArgumentType.Guid:
                    stringBuffer.Append('"');
                    stringBuffer.Append(*(Guid*)dataPointer, StringView.Empty);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(Guid);
                    break;

                case ArgumentType.DateTime:
                    stringBuffer.Append('"');
                    stringBuffer.Append(*(DateTime*)dataPointer, StringView.Empty);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(DateTime);
                    break;

                case ArgumentType.TimeSpan:
                    stringBuffer.Append('"');
                    stringBuffer.Append(*(TimeSpan*)dataPointer, StringView.Empty);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(TimeSpan);
                    break;

                case ArgumentType.Enum:
                    var enumArg = (EnumArg*)dataPointer;
                    stringBuffer.Append('"');
                    enumArg->AppendTo(stringBuffer);
                    stringBuffer.Append('"');
                    dataPointer += sizeof(EnumArg);
                    break;

                case ArgumentType.Null:
                    stringBuffer.Append("null");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendEscapedChar(char c, StringBuffer stringBuffer)
        {
            // Escape characters based on https://tools.ietf.org/html/rfc7159

            if (c == '\\' || c == '"' || c <= '\u001F')
                AppendControlChar(c, stringBuffer);
            else
                stringBuffer.Append(c);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AppendControlChar(char c, StringBuffer stringBuffer)
        {
            switch (c)
            {
                case '"':
                    stringBuffer.Append(@"\""");
                    break;

                case '\\':
                    stringBuffer.Append(@"\\");
                    break;

                case '\b':
                    stringBuffer.Append(@"\b");
                    break;

                case '\t':
                    stringBuffer.Append(@"\t");
                    break;

                case '\n':
                    stringBuffer.Append(@"\n");
                    break;

                case '\f':
                    stringBuffer.Append(@"\f");
                    break;

                case '\r':
                    stringBuffer.Append(@"\r");
                    break;

                default:
                    stringBuffer.Append(@"\u00");
                    var byteValue = unchecked((byte)c);
                    HexUtils.AppendValueAsHex(stringBuffer, &byteValue, 1);
                    break;
            }
        }
    }
}
