using System;

namespace ZeroLog;

[Flags]
internal enum ArgumentType : byte
{
    None,

    String,
    Null,
    Char,
    Boolean,

    Byte,
    SByte,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    IntPtr,
    UIntPtr,
    Single,
    Double,
    Decimal,

    Guid,
    DateTime,
    TimeSpan,
    DateOnly,
    TimeOnly,
    DateTimeOffset,
    StringSpan,
    Utf8StringSpan,
    Enum,
    Unmanaged,

    KeyString,
    Chain,
    EndOfTruncatedMessage,

    FormatFlag = 1 << 7
}

internal static class ArgumentTypeExtensions
{
    extension(ArgumentType)
    {
        public static ArgumentType FromValueType<T>()
            where T : unmanaged
        {
            return typeof(T) == typeof(bool) ? ArgumentType.Boolean
                : typeof(T) == typeof(byte) ? ArgumentType.Byte
                : typeof(T) == typeof(sbyte) ? ArgumentType.SByte
                : typeof(T) == typeof(short) ? ArgumentType.Int16
                : typeof(T) == typeof(ushort) ? ArgumentType.UInt16
                : typeof(T) == typeof(int) ? ArgumentType.Int32
                : typeof(T) == typeof(uint) ? ArgumentType.UInt32
                : typeof(T) == typeof(long) ? ArgumentType.Int64
                : typeof(T) == typeof(ulong) ? ArgumentType.UInt64
                : typeof(T) == typeof(nint) ? ArgumentType.IntPtr
                : typeof(T) == typeof(nuint) ? ArgumentType.UIntPtr
                : typeof(T) == typeof(float) ? ArgumentType.Single
                : typeof(T) == typeof(double) ? ArgumentType.Double
                : typeof(T) == typeof(decimal) ? ArgumentType.Decimal
                : typeof(T) == typeof(Guid) ? ArgumentType.Guid
                : typeof(T) == typeof(DateTime) ? ArgumentType.DateTime
                : typeof(T) == typeof(TimeSpan) ? ArgumentType.TimeSpan
#if NET
                : typeof(T) == typeof(DateOnly) ? ArgumentType.DateOnly
                : typeof(T) == typeof(TimeOnly) ? ArgumentType.TimeOnly
#endif
                : typeof(T) == typeof(DateTimeOffset) ? ArgumentType.DateTimeOffset
                : typeof(T) == typeof(char) ? ArgumentType.Char
                : ArgumentType.None;
        }
    }

    public static bool IsNumeric(this ArgumentType argType)
    {
        switch (argType)
        {
            case ArgumentType.Byte:
            case ArgumentType.SByte:
            case ArgumentType.Int16:
            case ArgumentType.UInt16:
            case ArgumentType.Int32:
            case ArgumentType.UInt32:
            case ArgumentType.Int64:
            case ArgumentType.UInt64:
            case ArgumentType.IntPtr:
            case ArgumentType.UIntPtr:
            case ArgumentType.Single:
            case ArgumentType.Double:
            case ArgumentType.Decimal:
                return true;

            default:
                return false;
        }
    }
}
