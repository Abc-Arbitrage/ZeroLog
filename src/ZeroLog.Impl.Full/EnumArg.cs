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
internal struct EnumArg
{
    private IntPtr _typeHandle;
    private ulong _value;

    public EnumArg(IntPtr typeHandle, ulong value)
    {
        _typeHandle = typeHandle;
        _value = value;
    }

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

        var type = TypeUtil.GetTypeFromHandle(_typeHandle);
        if (type is null)
            return null;

        LogManager.RegisterEnum(type);
        return EnumCache.GetString(_typeHandle, _value, out _);
    }
}
