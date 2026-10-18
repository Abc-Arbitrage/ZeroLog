namespace ZeroLog;

internal static class AppendChain
{
    public const ushort Version = 1;

    public enum LinkType : ushort
    {
        // /!\ These values MUST remain consistent between versions

        None = 0,

        String = 1,
        Null = 2,
        Boolean = 3,
        Char = 4,

        SByte = 5,
        Byte = 6,
        Int16 = 7,
        UInt16 = 8,
        Int32 = 9,
        UInt32 = 10,
        Int64 = 11,
        UInt64 = 12,
        Decimal = 13,
        Single = 14,
        Double = 15,
    }
}
