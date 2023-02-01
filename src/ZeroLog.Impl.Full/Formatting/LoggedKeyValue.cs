using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZeroLog.Formatting;

/// <summary>
/// Represents a logged key/value metadata item.
/// </summary>
public readonly unsafe ref struct LoggedKeyValue
{
    private readonly ReadOnlySpan<byte> _rawData;

    /// <summary>
    /// The item key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// The item value.
    /// </summary>
    public ReadOnlySpan<char> Value { get; }

    /// <summary>
    /// Indicates whether the value is null.
    /// </summary>
    public bool IsNull => ArgumentType == ArgumentType.Null;

    /// <summary>
    /// Indicates whether the value is a boolean.
    /// </summary>
    public bool IsBoolean => ArgumentType == ArgumentType.Boolean;

    /// <summary>
    /// Indicates whether the value is numeric (integer, floating point or decimal).
    /// </summary>
    public bool IsNumeric => ArgumentType.IsNumeric();

    internal LoggedKeyValue(string key, ReadOnlySpan<char> value, ReadOnlySpan<byte> rawData)
    {
        _rawData = rawData;
        Key = key;
        Value = value;
    }

    /// <summary>
    /// Returns a string which represents the key/value pair.
    /// </summary>
    public override string ToString()
        => $"{Key} = {Value}";

    internal ArgumentType ArgumentType => MemoryMarshal.Read<ArgumentType>(_rawData);

    /// <summary>
    /// The type of the item value.
    /// </summary>
    /// <seealso cref="TryGetValue{T}"/>
    public Type? ValueType => ArgumentType switch
    {
        ArgumentType.Char           => typeof(char),
        ArgumentType.Boolean        => typeof(bool),
        ArgumentType.Byte           => typeof(byte),
        ArgumentType.SByte          => typeof(sbyte),
        ArgumentType.Int16          => typeof(short),
        ArgumentType.UInt16         => typeof(ushort),
        ArgumentType.Int32          => typeof(int),
        ArgumentType.UInt32         => typeof(uint),
        ArgumentType.Int64          => typeof(long),
        ArgumentType.UInt64         => typeof(ulong),
        ArgumentType.IntPtr         => typeof(nint),
        ArgumentType.UIntPtr        => typeof(nuint),
        ArgumentType.Single         => typeof(float),
        ArgumentType.Double         => typeof(double),
        ArgumentType.Decimal        => typeof(decimal),
        ArgumentType.Guid           => typeof(Guid),
        ArgumentType.DateTime       => typeof(DateTime),
        ArgumentType.TimeSpan       => typeof(TimeSpan),
        ArgumentType.DateOnly       => typeof(DateOnly),
        ArgumentType.TimeOnly       => typeof(TimeOnly),
        ArgumentType.String         => typeof(string),
        ArgumentType.StringSpan     => typeof(string),
        ArgumentType.Utf8StringSpan => typeof(string),
        ArgumentType.Enum           => MemoryMarshal.Read<EnumArg>(_rawData[sizeof(ArgumentType)..]).Type,
        ArgumentType.Unmanaged      => MemoryMarshal.Read<UnmanagedArgHeader>(_rawData[sizeof(ArgumentType)..]).Type,
        _                           => null
    };

    /// <summary>
    /// Tries to get the item value as a struct of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="result">The value.</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>True if the value was of type <typeparamref name="T"/> and could be extracted.</returns>
    /// <seealso cref="ValueType"/>
    public bool TryGetValue<T>(out T result)
        where T : struct
    {
        if (_rawData.IsEmpty)
        {
            result = default;
            return false;
        }

        fixed (byte* rawDataPointer = _rawData)
        {
            var valueType = *(ArgumentType*)rawDataPointer; // There is no FormatFlag in this context
            var dataPointer = rawDataPointer + sizeof(ArgumentType);

            switch (valueType)
            {
                case ArgumentType.Char when typeof(T) == typeof(char):
                case ArgumentType.Boolean when typeof(T) == typeof(bool):
                case ArgumentType.Byte when typeof(T) == typeof(byte):
                case ArgumentType.SByte when typeof(T) == typeof(sbyte):
                case ArgumentType.Int16 when typeof(T) == typeof(short):
                case ArgumentType.UInt16 when typeof(T) == typeof(ushort):
                case ArgumentType.Int32 when typeof(T) == typeof(int):
                case ArgumentType.UInt32 when typeof(T) == typeof(uint):
                case ArgumentType.Int64 when typeof(T) == typeof(long):
                case ArgumentType.UInt64 when typeof(T) == typeof(ulong):
                case ArgumentType.IntPtr when typeof(T) == typeof(nint):
                case ArgumentType.UIntPtr when typeof(T) == typeof(nuint):
                case ArgumentType.Single when typeof(T) == typeof(float):
                case ArgumentType.Double when typeof(T) == typeof(double):
                case ArgumentType.Decimal when typeof(T) == typeof(decimal):
                case ArgumentType.Guid when typeof(T) == typeof(Guid):
                case ArgumentType.DateTime when typeof(T) == typeof(DateTime):
                case ArgumentType.TimeSpan when typeof(T) == typeof(TimeSpan):
                case ArgumentType.DateOnly when typeof(T) == typeof(DateOnly):
                case ArgumentType.TimeOnly when typeof(T) == typeof(TimeOnly):
                {
                    result = Unsafe.Read<T>(dataPointer);
                    return true;
                }

                case ArgumentType.Enum:
                {
                    var argPtr = (EnumArg*)dataPointer;
                    return argPtr->TryGetValue<T>(out result);
                }

                case ArgumentType.Unmanaged:
                {
                    var argPtr = (UnmanagedArgHeader*)dataPointer;
                    if (typeof(T) != argPtr->Type)
                        break;

                    result = Unsafe.Read<T>(argPtr + sizeof(UnmanagedArgHeader));
                    return true;
                }
            }

            result = default;
            return false;
        }
    }
}
