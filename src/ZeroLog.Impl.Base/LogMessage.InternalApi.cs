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
    public static partial class InternalApi
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChain(LogMessage message, Chain chain)
            => message.InternalAppendChain(chain);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainItem(LogMessage message, string? value)
            => message.InternalAppendChainItem(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainItem<T>(LogMessage message, T value)
            where T : unmanaged
            => message.InternalAppendChainItem(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendChainItem<T>(LogMessage message, T? value)
            where T : unmanaged
            => message.InternalAppendChainItem(value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract partial class Chain
        {
            protected abstract void Execute(ChainExecutionContext context);
        }

        public sealed partial class ChainExecutionContext
        {
            public partial void ConsumeString();
            public partial void ConsumeString(string? value);

            public partial void ConsumeValueType<T>()
                where T : unmanaged;

            public partial void SetNullable();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChain(InternalApi.Chain chain);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainItem(string? value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainItem<T>(T value)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainItem<T>(T? value)
        where T : unmanaged;

#if NETSTANDARD

    internal partial void InternalAppendChain(InternalApi.Chain chain)
    { }

    internal partial void InternalAppendChainItem(string? value)
    { }

    internal partial void InternalAppendChainItem<T>(T value)
        where T : unmanaged
    { }

    internal partial void InternalAppendChainItem<T>(T? value)
        where T : unmanaged
    { }

    partial class InternalApi
    {
        partial class Chain;

        public sealed partial class ChainExecutionContext
        {
            public partial void ConsumeString()
            { }

            public partial void ConsumeString(string? value)
            { }

            public partial void ConsumeValueType<T>()
                where T : unmanaged
            { }

            public partial void SetNullable()
            { }
        }
    }

#endif
}
