using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    internal static unsafe class StringBufferExtension
    {
        public static void Append(this StringBuffer stringBuffer, ref byte* dataPointer, StringView format, List<string> strings, List<IntPtr> argPointers)
        {
            var argument = *dataPointer;
            dataPointer += sizeof(ArgumentType);

            var argumentType = (ArgumentType)(argument & ArgumentTypeMask.ArgumentType);

            var hasFormatSpecifier = (argument & ArgumentTypeMask.FormatSpecifier) != 0;
            if (hasFormatSpecifier)
            {
                var formatSpecifier = strings[*dataPointer];
                dataPointer += sizeof(byte);

                fixed (char* p = formatSpecifier)
                {
                    var formatSpecifierView = new StringView(p, formatSpecifier.Length);
                    AppendArg(stringBuffer, ref dataPointer, argumentType, formatSpecifierView, strings, argPointers);
                }
            }
            else
            {
                AppendArg(stringBuffer, ref dataPointer, argumentType, format, strings, argPointers);
            }
        }

        private static void AppendArg(StringBuffer stringBuffer, ref byte* argPointer, ArgumentType argumentType, StringView format, List<string> strings, List<IntPtr> argPointers)
        {
            switch (argumentType)
            {
                case ArgumentType.String:
                    var stringIndex = *argPointer;
                    stringBuffer.Append(strings[stringIndex]);
                    argPointer += sizeof(byte);
                    break;

                case ArgumentType.AsciiString:
                    var length = *(int*)argPointer;
                    argPointer += sizeof(int);
                    stringBuffer.Append(new AsciiString(argPointer, length));
                    argPointer += length;
                    break;

                case ArgumentType.Boolean:
                    stringBuffer.Append(*(bool*)argPointer);
                    argPointer += sizeof(bool);
                    break;

                case ArgumentType.Byte:
                    stringBuffer.Append(*argPointer, format);
                    argPointer += sizeof(byte);
                    break;

                case ArgumentType.Char:
                    stringBuffer.Append(*(char*)argPointer);
                    argPointer += sizeof(char);
                    break;

                case ArgumentType.Int16:
                    stringBuffer.Append(*(short*)argPointer, format);
                    argPointer += sizeof(short);
                    break;

                case ArgumentType.Int32:
                    stringBuffer.Append(*(int*)argPointer, format);
                    argPointer += sizeof(int);
                    break;

                case ArgumentType.Int64:
                    stringBuffer.Append(*(long*)argPointer, format);
                    argPointer += sizeof(long);
                    break;

                case ArgumentType.Single:
                    stringBuffer.Append(*(float*)argPointer, format);
                    argPointer += sizeof(float);
                    break;

                case ArgumentType.Double:
                    stringBuffer.Append(*(double*)argPointer, format);
                    argPointer += sizeof(double);
                    break;

                case ArgumentType.Decimal:
                    stringBuffer.Append(*(decimal*)argPointer, format);
                    argPointer += sizeof(decimal);
                    break;

                case ArgumentType.Guid:
                    stringBuffer.Append(*(Guid*)argPointer, format);
                    argPointer += sizeof(Guid);
                    break;

                case ArgumentType.DateTime:
                    stringBuffer.Append(*(DateTime*)argPointer, format);
                    argPointer += sizeof(DateTime);
                    break;

                case ArgumentType.TimeSpan:
                    stringBuffer.Append(*(TimeSpan*)argPointer, format);
                    argPointer += sizeof(TimeSpan);
                    break;

                case ArgumentType.FormatString:
                    var argSet = new ArgSet(argPointers, strings);
                    stringBuffer.AppendArgSet(strings[*argPointer], ref argSet);
                    argPointer += sizeof(byte) + argSet.BytesRead;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
