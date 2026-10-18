using System.Text;

namespace ZeroLog;

internal static class AppendChain
{
    public const ushort Version = 1;

    public enum LinkType : ushort
    {
        // /!\ These values MUST remain consistent between versions

        None = 0,

        Null = 1,
        String = 2,
        ConstantString = 3,
        KeyString = 4,

        Boolean = 10,
        Char = 11,
        SByte = 12,
        Byte = 13,
        Int16 = 14,
        UInt16 = 15,
        Int32 = 16,
        UInt32 = 17,
        Int64 = 18,
        UInt64 = 19,
        Decimal = 20,
        Single = 21,
        Double = 22
    }

    public readonly struct Serializer()
    {
        private readonly StringBuilder _sb = new StringBuilder().Append((char)Version);

        public bool TryAddConstantString(string? constantString)
        {
            if (constantString is { Length: < ushort.MaxValue })
            {
                _sb.Append((char)LinkType.ConstantString);
                _sb.Append((char)constantString.Length);
                _sb.Append(constantString);
                return true;
            }

            return false;
        }

        public void AddLink(LinkType linkType)
            => _sb.Append((char)linkType);

        public override string ToString()
            => _sb.ToString();
    }
}
