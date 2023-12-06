using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZeroLog.Configuration;
using ZeroLog.Support;

namespace ZeroLog;

[StructLayout(LayoutKind.Sequential)]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
[SuppressMessage("ReSharper", "ReplaceWithPrimaryConstructorParameter")]
internal readonly struct EnumArg(IntPtr typeHandle, ulong value)
{
    private readonly IntPtr _typeHandle = typeHandle;
    private readonly ulong _value = value;

    public Type? Type => TypeUtil.GetTypeFromHandle(_typeHandle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<char> destination, out int charsWritten, ZeroLogConfiguration config)
    {
        var enumString = GetString(config);

        if (enumString != null)
        {
            if (enumString.Length <= destination.Length)
            {
                enumString.CopyTo(destination);
                charsWritten = enumString.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }

        return TryAppendNumericValue(destination, out charsWritten);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAppendNumericValue(Span<char> destination, out int charsWritten)
    {
        if (_value <= long.MaxValue || !EnumCache.IsEnumSigned(_typeHandle))
            return _value.TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);

        return unchecked((long)_value).TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? GetString(ZeroLogConfiguration config)
        => EnumCache.GetString(_typeHandle, _value, out var enumRegistered)
           ?? GetStringSlow(enumRegistered, config);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string? GetStringSlow(bool enumRegistered, ZeroLogConfiguration config)
    {
        if (enumRegistered || !config.AutoRegisterEnums)
            return null;

        if (Type is not { } type)
            return null;

        LogManager.RegisterEnum(type);
        return EnumCache.GetString(_typeHandle, _value, out _);
    }

    public bool TryGetValue<T>(out T result)
        where T : unmanaged
    {
        if (Type == typeof(T))
        {
            result = EnumCache.FromUInt64<T>(_value);
            return true;
        }

        result = default;
        return false;
    }
}
