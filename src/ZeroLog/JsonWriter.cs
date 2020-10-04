using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Formatting;

namespace ZeroLog
{
    public static unsafe class JsonWriter
    {
        public static void WriteJsonToStringBuffer(StringBuffer stringBuffer, IList<IntPtr> keyValuePointers,
                                                   string[] strings)
        {
            stringBuffer.Append(LogManager.Config.JsonSeparator);
            stringBuffer.Append("{ ");
            for (var i = 0; i < keyValuePointers.Count; i++)
            {
                var argPointer = (byte*)keyValuePointers[i].ToPointer();
                var dataPointer = argPointer;

                // Key.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);

                stringBuffer.Append(": ");

                // Value.
                AppendJsonValue(stringBuffer, strings, ref dataPointer);

                if (i != keyValuePointers.Count - 1)
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
                    if (NeedsEscaping(strings[*dataPointer]))
                    {
                        // This might be kind of slow, but should be relatively rare.
                        foreach (var c in strings[*dataPointer])
                        {
                            AppendEscapedJson(c, stringBuffer);
                        }
                    }
                    else
                    {
                        // There is nothing to escape, we can append the entire string in one go.
                        stringBuffer.Append(strings[*dataPointer]);
                    }

                    stringBuffer.Append('"');
                    dataPointer += sizeof(byte);
                    break;

                case ArgumentType.AsciiString:
                    var length = *(int*)dataPointer;
                    dataPointer += sizeof(int);
                    stringBuffer.Append('"');
                    stringBuffer.Append(new AsciiString(dataPointer, length));
                    stringBuffer.Append('"');
                    dataPointer += length;
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
                    dataPointer += sizeof(EnumArg);
                    enumArg->AppendTo(stringBuffer);
                    break;

                case ArgumentType.Null:
                    stringBuffer.Append("null");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static bool NeedsEscaping(string s)
        {
            // Not using linq here since it (might) add overhead.
            foreach (var c in s)
            {
                if (c < '\u001F' || c == '"' || c == '/')
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendEscapedJson(char c, StringBuffer stringBuffer)
        {
            // Escape characters based on https://tools.ietf.org/html/rfc7159
            switch (c)
            {
                case '/':
                    stringBuffer.Append("\\/");
                    break;

                case '"':
                    stringBuffer.Append("\\\"");
                    break;

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
                    stringBuffer.Append(c);
                    break;
            }
        }
    }
}
