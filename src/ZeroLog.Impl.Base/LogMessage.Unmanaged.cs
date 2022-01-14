using System.Runtime.CompilerServices;

namespace ZeroLog;

partial class LogMessage
{
    public LogMessage AppendUnmanaged<T>(T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    public LogMessage AppendUnmanaged<T>(T? value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

    public LogMessage AppendUnmanaged<T>(ref T value, string? format = null)
        where T : unmanaged
    {
        InternalAppendUnmanaged(ref value, format);
        return this;
    }

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
