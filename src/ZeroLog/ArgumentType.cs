using System;

namespace ZeroLog;

[Flags]
internal enum ArgumentType : byte
{
    None,
    String,
    Null,
    Boolean,
    Byte,
    SByte,
    Char,
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
    AsciiString,
    Enum,
    Unmanaged,
    KeyString,
    EndOfTruncatedMessage,

    FormatFlag = 1 << 7
}
