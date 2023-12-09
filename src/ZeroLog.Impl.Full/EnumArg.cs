﻿using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryFormat(Span<byte> destination, out int bytesWritten, ZeroLogConfiguration config)
    {
        var enumString = GetUtf8String(config);

        if (enumString != null)
        {
            if (enumString.Length <= destination.Length)
            {
                enumString.CopyTo(destination);
                bytesWritten = enumString.Length;
                return true;
            }

            bytesWritten = 0;
            return false;
        }

        return TryAppendNumericValue(destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAppendNumericValue(Span<char> destination, out int charsWritten)
    {
        if (_value <= long.MaxValue || !EnumCache.IsEnumSigned(_typeHandle))
            return _value.TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);

        return unchecked((long)_value).TryFormat(destination, out charsWritten, default, CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool TryAppendNumericValue(Span<byte> destination, out int bytesWritten)
    {
#if NET8_0_OR_GREATER
        if (_value <= long.MaxValue || !EnumCache.IsEnumSigned(_typeHandle))
            return _value.TryFormat(destination, out bytesWritten, default, CultureInfo.InvariantCulture);

        return unchecked((long)_value).TryFormat(destination, out bytesWritten, default, CultureInfo.InvariantCulture);
#else
        Span<char> buffer = stackalloc char[32];

        if (TryAppendNumericValue(buffer, out var charsWritten))
            return Utf8.FromUtf16(buffer.Slice(0, charsWritten), destination, out _, out bytesWritten) == OperationStatus.Done;

        bytesWritten = 0;
        return false;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? GetString(ZeroLogConfiguration config)
        => EnumCache.GetString(_typeHandle, _value, out var enumRegistered)
           ?? GetStringSlow(enumRegistered, config);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[]? GetUtf8String(ZeroLogConfiguration config)
        => EnumCache.GetUtf8String(_typeHandle, _value, out var enumRegistered)
           ?? GetUtf8StringSlow(enumRegistered, config);

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    private byte[]? GetUtf8StringSlow(bool enumRegistered, ZeroLogConfiguration config)
    {
        if (enumRegistered || !config.AutoRegisterEnums)
            return null;

        if (Type is not { } type)
            return null;

        LogManager.RegisterEnum(type);
        return EnumCache.GetUtf8String(_typeHandle, _value, out _);
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
