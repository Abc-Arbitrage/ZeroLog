using System;

namespace ZeroLog.Formatting;

public readonly ref struct FormattedKeyValue
{
    public string Key { get; }
    public ReadOnlySpan<char> Value { get; }
    private ArgumentType ValueType { get; }

    public bool IsBoolean => ValueType == ArgumentType.Boolean;
    public bool IsNull => ValueType == ArgumentType.Null;

    public bool IsNumeric
    {
        get
        {
            switch (ValueType)
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

    internal FormattedKeyValue(string key, ReadOnlySpan<char> value, ArgumentType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }

    public override string ToString()
        => $"{Key} = {Value}";
}
