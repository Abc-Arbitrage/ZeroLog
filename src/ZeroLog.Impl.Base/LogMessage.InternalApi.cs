using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static partial class InternalApi
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract partial class Chain
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendChainEnumItem<T>(LogMessage message, T value)
                where T : struct, Enum
                => message.InternalAppendChainEnumItem(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendChainEnumItem<T>(LogMessage message, T? value)
                where T : struct, Enum
                => message.InternalAppendChainEnumItem(value);

            protected abstract void Execute(ref ChainExecutionContext context);

            internal void CallExecute(ref ChainExecutionContext context)
                => Execute(ref context);

            public ref partial struct ChainExecutionContext
            {
                public partial void OutputString();
                public partial void OutputString(string? value);

                public partial void OutputValueType<T>(bool nullable)
                    where T : unmanaged;

                public partial void OutputEnum<T>(bool nullable)
                    where T : struct, Enum;

                public partial void SetFormatString();
                public partial void SetFormatString(string? format);

                public partial void SetKeyString();
                public partial void SetKeyString(string? key);
            }
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainEnumItem<T>(T value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainEnumItem<T>(T? value)
        where T : struct, Enum;

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

    internal partial void InternalAppendChainEnumItem<T>(T value)
        where T : struct, Enum
    { }

    internal partial void InternalAppendChainEnumItem<T>(T? value)
        where T : struct, Enum
    { }

    partial class InternalApi
    {
        partial class Chain
        {
            ref partial struct ChainExecutionContext
            {
                public partial void OutputString() { }
                public partial void OutputString(string? value) { }
                public partial void OutputValueType<T>(bool nullable) where T : unmanaged { }
                public partial void OutputEnum<T>(bool nullable) where T : struct, Enum { }

                public partial void SetFormatString() { }
                public partial void SetFormatString(string? format) { }

                public partial void SetKeyString() { }
                public partial void SetKeyString(string? key) { }
            }
        }
    }

#endif
}
