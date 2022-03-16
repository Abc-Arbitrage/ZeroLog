using System;

namespace ZeroLog.Formatting;

/// <summary>
/// Represents a logged key/value metadata item.
/// </summary>
public readonly ref struct LoggedKeyValue
{
    /// <summary>
    /// The item key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The item value.
    /// </summary>
    public ReadOnlySpan<char> Value { get; }

    private ArgumentType ValueType { get; }

    /// <summary>
    /// Indicates whether the value is null.
    /// </summary>
    public bool IsNull => ValueType == ArgumentType.Null;

    /// <summary>
    /// Indicates whether the value is a boolean.
    /// </summary>
    public bool IsBoolean => ValueType == ArgumentType.Boolean;

    /// <summary>
    /// Indicates whether the value is numeric (integer, floating point or decimal).
    /// </summary>
    public bool IsNumeric => ValueType.IsNumeric();

    internal LoggedKeyValue(string key, ReadOnlySpan<char> value, ArgumentType valueType)
    {
        Key = key;
        Value = value;
        ValueType = valueType;
    }

    /// <summary>
    /// Returns a string which represents the key/value pair.
    /// </summary>
    public override string ToString()
        => $"{Key} = {Value}";
}
