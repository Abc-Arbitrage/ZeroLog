using System;

namespace ZeroLog.Formatting;

public readonly ref struct LoggedKeyValue
{
    public string Key { get; }
    public ReadOnlySpan<char> Value { get; }
    private ArgumentType ValueType { get; }

    public bool IsNull => ValueType == ArgumentType.Null;
    public bool IsBoolean => ValueType == ArgumentType.Boolean;
    public bool IsNumeric => ValueType.IsNumeric();

    internal LoggedKeyValue(string key, ReadOnlySpan<char> value, ArgumentType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }

    public override string ToString()
        => $"{Key} = {Value}";
}
