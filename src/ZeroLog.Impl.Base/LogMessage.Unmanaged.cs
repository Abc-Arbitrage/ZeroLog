using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ZeroLog;

[SuppressMessage("ReSharper", "UnusedParameterInPartialMethod")]
partial class LogMessage
{
    /// <summary>
    /// Appends an unmanaged value to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public LogMessage AppendUnmanaged<T>(T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    /// <summary>
    /// Appends a nullable unmanaged value to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public LogMessage AppendUnmanaged<T>(T? value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    /// <summary>
    /// Appends an unmanaged value to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public LogMessage AppendUnmanaged<T>(ref T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    /// <summary>
    /// Appends a nullable unmanaged value to the message.
    /// </summary>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format string.</param>
    /// <typeparam name="T">The value type.</typeparam>
    public LogMessage AppendUnmanaged<T>(ref T? value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendUnmanaged<T>(ref T value, string? format)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private partial void InternalAppendUnmanaged<T>(ref T? value, string? format)
        where T : unmanaged;

#if NETSTANDARD

    private partial void InternalAppendUnmanaged<T>(ref T value, string? format)
        where T : unmanaged
    {
    }

    private partial void InternalAppendUnmanaged<T>(ref T? value, string? format)
        where T : unmanaged
    {
    }

#endif
}
