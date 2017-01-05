using System;
using System.Collections.Generic;
using System.Text.Formatting;

namespace ZeroLog
{
    public static unsafe class StringBufferExtension
    {
        public static int AppendFrom(this StringBuffer stringBuffer, byte[] buffer, int offset, StringView format, List<string> strings)
        {
            var argumentType = (ArgumentType)buffer[offset++];

            switch (argumentType)
            {
                case ArgumentType.String:
                    var stringIndex = buffer[offset];
                    stringBuffer.Append(strings[stringIndex]);
                    return 2 * sizeof(byte);

                case ArgumentType.BooleanTrue:
                    stringBuffer.Append(true);
                    return sizeof(byte);

                case ArgumentType.BooleanFalse:
                    stringBuffer.Append(false);
                    return sizeof(byte);

                case ArgumentType.Byte:
                    stringBuffer.Append(buffer[offset], StringView.Empty);
                    return 2 * sizeof(byte);

                case ArgumentType.Char:
                    stringBuffer.Append(BitConverter.ToChar(buffer, offset));
                    return sizeof(byte) + sizeof(char);

                case ArgumentType.Int16:
                    stringBuffer.Append(BitConverter.ToInt16(buffer, offset), format);
                    return sizeof(byte) + sizeof(short);

                case ArgumentType.Int32:
                    stringBuffer.Append(BitConverter.ToInt32(buffer, offset), format);
                    return sizeof(byte) + sizeof(int);

                case ArgumentType.Int64:
                    stringBuffer.Append(BitConverter.ToInt64(buffer, offset), format);
                    return sizeof(byte) + sizeof(long);

                case ArgumentType.Single:
                    stringBuffer.Append(BitConverter.ToSingle(buffer, offset), format);
                    return sizeof(byte) + sizeof(float);

                case ArgumentType.Double:
                    stringBuffer.Append(BitConverter.ToDouble(buffer, offset), format);
                    return sizeof(byte) + sizeof(double);

                case ArgumentType.Decimal:
                    stringBuffer.Append(ReadDecimal(buffer, offset), format);
                    return sizeof(byte) + sizeof(decimal);

                case ArgumentType.Guid:
                    var guid = ReadGuid(buffer, offset);
                    throw new NotImplementedException(); //TODO
                //return sizeof(byte) + sizeof(Guid);

                case ArgumentType.DateTime:
                    var dateTime = ReadDateTime(buffer, offset);
                    throw new NotImplementedException(); //TODO
                //return sizeof(byte) + sizeof(ulong);

                case ArgumentType.TimeSpan:
                    var timeSpan = ReadTimeSpan(buffer, offset);
                    throw new NotImplementedException(); //TODO
                //return sizeof(byte) + sizeof(long);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static decimal ReadDecimal(byte[] buffer, int offset)
        {
            fixed (byte* pbyte = buffer)
                return *(decimal*)(pbyte + offset);
        }

        private static Guid ReadGuid(byte[] buffer, int offset)
        {
            fixed (byte* pbyte = buffer)
                return *(Guid*)(pbyte + offset);
        }

        private static DateTime ReadDateTime(byte[] buffer, int offset)
        {
            var dateData = BitConverter.ToUInt64(buffer, offset);
            var ticks = (long)(dateData & 0x3FFFFFFFFFFFFFFF);
            var kind = (DateTimeKind)(dateData & 0xC000000000000000);
            return new DateTime(ticks, kind);
        }

        private static TimeSpan ReadTimeSpan(byte[] buffer, int offset)
        {
            var ticks = BitConverter.ToInt64(buffer, offset);
            return new TimeSpan(ticks);
        }
    }
}