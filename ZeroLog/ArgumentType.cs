namespace ZeroLog
{
    internal enum ArgumentType : byte
    {
        FormatString,
        String,
        Boolean,
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
    }

    internal static class ArgumentTypeMask
    {
        public const byte ArgumentType = 0x7F;
        public const byte FormatSpecifier = 0x80;
    }
}