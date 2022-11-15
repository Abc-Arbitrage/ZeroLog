using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System;

internal readonly struct Index : IEquatable<Index>
{
    private readonly int _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Index(int value, bool fromEnd = false)
    {
        if (value < 0)
            ThrowValueArgumentOutOfRange_NeedNonNegNumException();

        _value = fromEnd ? ~value : value;
    }

    private Index(int value)
    {
        _value = value;
    }

    public static Index Start => new(0);
    public static Index End => new(~0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromStart(int value)
    {
        if (value < 0)
            ThrowValueArgumentOutOfRange_NeedNonNegNumException();

        return new Index(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Index FromEnd(int value)
    {
        if (value < 0)
            ThrowValueArgumentOutOfRange_NeedNonNegNumException();

        return new Index(~value);
    }

    public int Value => _value < 0 ? ~_value : _value;
    public bool IsFromEnd => _value < 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetOffset(int length)
    {
        var offset = _value;
        if (IsFromEnd)
            offset += length + 1;
        return offset;
    }

    public override bool Equals(object? value)
        => value is Index index && _value == index._value;

    public bool Equals(Index other)
        => _value == other._value;

    public override int GetHashCode()
        => _value;

    public static implicit operator Index(int value)
        => FromStart(value);

    public override string ToString()
        => IsFromEnd
            ? '^' + Value.ToString(CultureInfo.InvariantCulture)
            : ((uint)Value).ToString(CultureInfo.InvariantCulture);

    [SuppressMessage("ReSharper", "NotResolvedInText")]
    private static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
        => throw new ArgumentOutOfRangeException("value", "Non-negative number required.");
}
