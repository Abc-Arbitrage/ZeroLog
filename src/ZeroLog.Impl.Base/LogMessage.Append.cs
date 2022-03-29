using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

[SuppressMessage("ReSharper", "UnusedParameterInPartialMethod")]
partial class LogMessage
{
    /// <summary>
    /// Appends a value of type string to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public LogMessage Append(string? value)
    {
        InternalAppendString(value);
        return this;
    }

    /// <summary>
    /// Appends an enum to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public LogMessage AppendEnum<T>(T value)
        where T : struct, Enum
    {
        InternalAppendEnum(value);
        return this;
    }

    /// <summary>
    /// Appends a nullable enum to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public LogMessage AppendEnum<T>(T? value)
        where T : struct, Enum
    {
        InternalAppendEnum(value);
        return this;
    }

    /// <summary>
    /// Appends an ASCII string to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public LogMessage AppendAsciiString(ReadOnlySpan<char> value)
    {
        InternalAppendAsciiString(value);
        return this;
    }

    /// <summary>
    /// Appends an ASCII string represented as bytes to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public LogMessage AppendAsciiString(ReadOnlySpan<byte> value)
    {
        InternalAppendAsciiString(value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendString(string? value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendValueType<T>(T value, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendValueType<T>(T? value, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendValueType<T>(T value, string format, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendValueType<T>(T? value, string format, ArgumentType argType)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendEnum<T>(T value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendEnum<T>(T? value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendAsciiString(ReadOnlySpan<char> value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendAsciiString(ReadOnlySpan<byte> value);

#if NETSTANDARD

    internal partial void InternalAppendString(string? value)
    {
    }

    internal partial void InternalAppendValueType<T>(T value, ArgumentType argType)
        where T : unmanaged
    {
    }

    internal partial void InternalAppendValueType<T>(T? value, ArgumentType argType)
        where T : unmanaged
    {
    }

    internal partial void InternalAppendValueType<T>(T value, string format, ArgumentType argType)
        where T : unmanaged
    {
    }

    internal partial void InternalAppendValueType<T>(T? value, string format, ArgumentType argType)
        where T : unmanaged
    {
    }

    internal partial void InternalAppendEnum<T>(T value)
        where T : struct, Enum
    {
    }

    internal partial void InternalAppendEnum<T>(T? value)
        where T : struct, Enum
    {
    }

    private partial void InternalAppendAsciiString(ReadOnlySpan<char> value)
    {
    }

    private partial void InternalAppendAsciiString(ReadOnlySpan<byte> value)
    {
    }

#endif
}
