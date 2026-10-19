using System;
using System.Diagnostics;
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
        ConstantKeyString = 4,
        VariableKeyString = 5,

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

    extension(LinkType linkType)
    {
        public bool IsFixedSize => linkType is not (LinkType.ConstantString or LinkType.ConstantKeyString);
    }

#if ZEROLOG_ANALYZERS

    public readonly ref struct Serializer
    {
        private readonly StringBuilder _output = new();
        private readonly StringBuilder _constantStringBuffer = new();

        public Serializer()
        {
            _output.Append((char)Version);
        }

        public void AddFixedSizeLink(LinkType linkType)
        {
            Debug.Assert(linkType.IsFixedSize);

            FlushConstantString();
            _output.Append((char)linkType);
        }

        public void AddConstantString(string? constantString)
        {
            if (constantString is null)
            {
                AddFixedSizeLink(LinkType.Null);
                return;
            }

            _constantStringBuffer.Append(constantString);
        }

        public void AddConstantKeyString(string? constantString)
        {
            FlushConstantString();

            _constantStringBuffer.Append(constantString ?? string.Empty);

            if (_constantStringBuffer.Length > ushort.MaxValue)
            {
                AddFixedSizeLink(LinkType.None); // This is unreasonable anyway
                return;
            }

            _output.Append((char)LinkType.ConstantKeyString)
                   .Append((char)_constantStringBuffer.Length)
                   .Append(_constantStringBuffer);

            _constantStringBuffer.Clear();
        }

        private void FlushConstantString()
        {
            if (_constantStringBuffer.Length == 0)
                return;

            if (_constantStringBuffer.Length <= ushort.MaxValue)
            {
                _output.Append((char)LinkType.ConstantString)
                       .Append((char)_constantStringBuffer.Length)
                       .Append(_constantStringBuffer);
            }
            else
            {
                for (var position = 0; position < _constantStringBuffer.Length;)
                {
                    var length = Math.Min(ushort.MaxValue, _constantStringBuffer.Length - position);

                    _output.Append((char)LinkType.ConstantString)
                           .Append((char)length)
                           .Append(_constantStringBuffer.ToString(position, length));

                    position += length;
                }
            }

            _constantStringBuffer.Clear();
        }

        public string ToSerializedResult()
        {
            FlushConstantString();
            return _output.ToString();
        }
    }

#endif
}
