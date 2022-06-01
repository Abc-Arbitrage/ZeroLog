using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

[SuppressMessage("ReSharper", "UnusedParameterInPartialMethod")]
partial class LogMessage
{
    /// <summary>
    /// Appends a value of type string to the message metadata.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public LogMessage AppendKeyValue(string key, string? value)
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    /// <summary>
    /// Appends a value of enum type to the message metadata.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public LogMessage AppendKeyValue<T>(string key, T value)
        where T : struct, Enum
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    /// <summary>
    /// Appends a value of nullable enum type to the message metadata.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public LogMessage AppendKeyValue<T>(string key, T? value)
        where T : struct, Enum
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    /// <summary>
    /// Appends a value of type string span to the message metadata. This will copy the span and use buffer space.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public LogMessage AppendKeyValue(string key, ReadOnlySpan<char> value)
    {
        InternalAppendKeyValue(key, value);
        return this;
    }

    /// <summary>
    /// Appends an UTF-8 string to the message metadata. This will copy the span and use buffer space.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public LogMessage AppendKeyValue(string key, ReadOnlySpan<byte> value)
    {
        InternalAppendKeyValue(key, value);
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
    private partial void InternalAppendKeyValue(string key, ReadOnlySpan<char> value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendKeyValue(string key, ReadOnlySpan<byte> value);

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

    private partial void InternalAppendKeyValue(string key, ReadOnlySpan<char> value)
    {
    }

    private partial void InternalAppendKeyValue(string key, ReadOnlySpan<byte> value)
    {
    }

#endif
}
