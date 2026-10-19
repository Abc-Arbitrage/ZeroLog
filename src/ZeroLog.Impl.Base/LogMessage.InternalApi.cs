using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZeroLog;

#pragma warning disable CS1591

partial class LogMessage
{
    /// <summary>
    /// This class is for internal use only and should not be used directly by users.
    /// It may cause breaking changes or be removed in future versions without notice.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class InternalApi
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChain(LogMessage message, string chain)
            => message.InternalAppendChain(chain);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainLinkValue(LogMessage message, string? value)
            => message.InternalAppendChainLinkValue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainLinkValue<T>(LogMessage message, T value)
            where T : unmanaged
            => message.InternalAppendChainLinkValue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainLinkValue<T>(LogMessage message, T? value)
            where T : unmanaged
            => message.InternalAppendChainLinkValue(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChain(string chain);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainLinkValue(string? value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainLinkValue<T>(T value)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainLinkValue<T>(T? value)
        where T : unmanaged;

#if NETSTANDARD

    internal partial void InternalAppendChain(string chain)
    { }

    internal partial void InternalAppendChainLinkValue(string? value)
    { }

    internal partial void InternalAppendChainLinkValue<T>(T value)
        where T : unmanaged
    { }

    internal partial void InternalAppendChainLinkValue<T>(T? value)
        where T : unmanaged
    { }

#endif
}
