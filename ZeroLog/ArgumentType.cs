using System;

namespace ZeroLog
{
    enum ArgumentType : byte
    {
        String,
        BooleanTrue,
        BooleanFalse,
        Byte,
        Char,
        Int16,
        Int32,
        Int64,
        Single,
        Double,
        Decimal,
        Guid,
        DateTime,
        TimeSpan,

        Format,
    }
}