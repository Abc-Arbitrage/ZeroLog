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
    AsciiString,
    Enum,
    Unmanaged,

    KeyString,
    EndOfTruncatedMessage,

    FormatFlag = 1 << 7
}

internal static class ArgumentTypeExtensions
{
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
