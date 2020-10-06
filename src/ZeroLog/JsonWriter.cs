using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    internal static unsafe class JsonWriter
    {
        public static void WriteJsonToStringBuffer(StringBuffer stringBuffer, KeyValuePointerBuffer keyValuePointerBuffer,
                                                   string[] strings)
        {
            stringBuffer.Append(LogManager.Config.JsonSeparator);
            stringBuffer.Append("{ ");
            for (var i = 0; i < keyValuePointerBuffer.PointerCount; i++)
            {
                var argPointer = keyValuePointerBuffer.GetUnsafePointer(i);
                var dataPointer = argPointer;

                // Key.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);

                stringBuffer.Append(": ");

                // Value.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);

                if (i != keyValuePointerBuffer.PointerCount - 1)
                    stringBuffer.Append(", ");
            }

            stringBuffer.Append(" }");
        }

        private static void AppendJsonValue(StringBuffer stringBuffer, IReadOnlyList<string> strings, ref byte* dataPointer)
        {
            var argument = *dataPointer;
            dataPointer += sizeof(ArgumentType);

            var argumentType = (ArgumentType)(argument & ArgumentTypeMask.ArgumentType);

            switch (argumentType)
            {
                case ArgumentType.KeyString:
                case ArgumentType.String:
                    stringBuffer.Append('"');

                    foreach (var c in strings[*dataPointer])
                    {
                        AppendEscapedJson(c, stringBuffer);
                    }

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
                    AppendEscapedJson(*(char*)dataPointer, stringBuffer);
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

        private static void AppendEscapedJson(char c, StringBuffer stringBuffer)
        {
            // Escape characters based on https://tools.ietf.org/html/rfc7159
            if (c == '\\')
            {
                stringBuffer.Append("\\\\");
            }
            else if (c == '"')
            {
                stringBuffer.Append("\\\"");
            }
            else if (char.IsControl(c))
            {
                AppendControlChar(c, stringBuffer);
            }
            else
            {
                stringBuffer.Append(c);
            }
        }

        private static void AppendControlChar(char c, StringBuffer stringBuffer)
        {
            switch (c)
            {
                case '\u0000':
                    stringBuffer.Append("\\u0000");
                    break;

                case '\u0001':
                    stringBuffer.Append("\\u0001");
                    break;

                case '\u0002':
                    stringBuffer.Append("\\u0002");
                    break;

                case '\u0003':
                    stringBuffer.Append("\\u0003");
                    break;

                case '\u0004':
                    stringBuffer.Append("\\u0004");
                    break;

                case '\u0005':
                    stringBuffer.Append("\\u0005");
                    break;

                case '\u0006':
                    stringBuffer.Append("\\u0006");
                    break;

                case '\u0007':
                    stringBuffer.Append("\\u0007");
                    break;

                case '\u0008':
                    stringBuffer.Append("\\b");
                    break;

                case '\u0009':
                    stringBuffer.Append("\\t");
                    break;

                case '\u000A':
                    stringBuffer.Append("\\n");
                    break;

                case '\u000B':
                    stringBuffer.Append("\\u000B");
                    break;

                case '\u000C':
                    stringBuffer.Append("\\f");
                    break;

                case '\u000D':
                    stringBuffer.Append("\\r");
                    break;

                case '\u000E':
                    stringBuffer.Append("\\u000E");
                    break;

                case '\u000F':
                    stringBuffer.Append("\\u000F");
                    break;

                case '\u0010':
                    stringBuffer.Append("\\u0010");
                    break;

                case '\u0011':
                    stringBuffer.Append("\\u0011");
                    break;

                case '\u0012':
                    stringBuffer.Append("\\u0012");
                    break;

                case '\u0013':
                    stringBuffer.Append("\\u0013");
                    break;

                case '\u0014':
                    stringBuffer.Append("\\u0014");
                    break;

                case '\u0015':
                    stringBuffer.Append("\\u0015");
                    break;

                case '\u0016':
                    stringBuffer.Append("\\u0016");
                    break;

                case '\u0017':
                    stringBuffer.Append("\\u0017");
                    break;

                case '\u0018':
                    stringBuffer.Append("\\u0018");
                    break;

                case '\u0019':
                    stringBuffer.Append("\\u0019");
                    break;

                case '\u001A':
                    stringBuffer.Append("\\u001A");
                    break;

                case '\u001B':
                    stringBuffer.Append("\\u001B");
                    break;

                case '\u001C':
                    stringBuffer.Append("\\u001C");
                    break;

                case '\u001D':
                    stringBuffer.Append("\\u001D");
                    break;

                case '\u001E':
                    stringBuffer.Append("\\u001E");
                    break;

                case '\u001F':
                    stringBuffer.Append("\\u001F");
                    break;

                default:
                    // According to rfc7159, only control characters from U+0000 to U+001F must be escaped.
                    stringBuffer.Append(c);
                    break;
            }
        }
    }
}
