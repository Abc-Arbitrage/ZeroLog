using System;
using System.Runtime.CompilerServices;

namespace ZeroLog;

public sealed partial class LogMessage
{
    public LogMessage AppendKeyValue(string key, string? value)
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    public LogMessage AppendKeyValue<T>(string key, T value)
        where T : struct, Enum
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    public LogMessage AppendKeyValue<T>(string key, T? value)
        where T : struct, Enum
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    public LogMessage AppendKeyValueAscii(string key, ReadOnlySpan<char> value)
    {
        InternalAppendKeyValueAscii(key, value);
        return this;
    }

    public LogMessage AppendKeyValueAscii(string key, ReadOnlySpan<byte> value)
    {
        InternalAppendKeyValueAscii(key, value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue(string key, string? value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue<T>(string key, T value, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue<T>(string key, T? value, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue<T>(string key, T value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue<T>(string key, T? value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<char> value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<byte> value);

#if NETSTANDARD

    private partial void InternalAppendKeyValue(string key, string? value)
    {
    }

    private partial void InternalAppendKeyValue<T>(string key, T value, ArgumentType argType)
        where T : unmanaged
    {
    }

    private partial void InternalAppendKeyValue<T>(string key, T? value, ArgumentType argType)
        where T : unmanaged
    {
    }

    private partial void InternalAppendKeyValue<T>(string key, T value)
        where T : struct, Enum
    {
    }

    private partial void InternalAppendKeyValue<T>(string key, T? value)
        where T : struct, Enum
    {
    }

    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<char> value)
    {
    }

    private partial void InternalAppendKeyValueAscii(string key, ReadOnlySpan<byte> value)
    {
    }

#endif
}
