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
            public const int Version = 1;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public partial void Initialize();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendChain(LogMessage message, Chain chain)
                => message.InternalAppendChain(chain);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendValue(LogMessage message, string? value)
                => message.InternalAppendChainValue(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendValue<T>(LogMessage message, T value)
                where T : unmanaged
                => message.InternalAppendChainValue(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendValue<T>(LogMessage message, T? value)
                where T : unmanaged
                => message.InternalAppendChainValue(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendEnumValue<T>(LogMessage message, T value)
                where T : struct, Enum
                => message.InternalAppendChainEnumValue(value);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void AppendEnumValue<T>(LogMessage message, T? value)
                where T : struct, Enum
                => message.InternalAppendChainEnumValue(value);

            protected abstract void Execute(ref ChainExecutionContext context);

            internal void CallExecute(ref ChainExecutionContext context)
                => Execute(ref context);

            public ref partial struct ChainExecutionContext
            {
                public partial void OutputString();
                public partial void OutputString(string? value);

                public partial void OutputValueType<T>()
                    where T : unmanaged;

                public partial void OutputNullableValueType<T>()
                    where T : unmanaged;

                public partial void OutputEnum();
                public partial void OutputNullableEnum();

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
    internal partial void InternalAppendChainValue(string? value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainValue<T>(T value)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainValue<T>(T? value)
        where T : unmanaged;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainEnumValue<T>(T value)
        where T : struct, Enum;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal partial void InternalAppendChainEnumValue<T>(T? value)
        where T : struct, Enum;

#if NETSTANDARD
    internal partial void InternalAppendChain(InternalApi.Chain chain)
    { }

    internal partial void InternalAppendChainValue(string? value)
    { }

    internal partial void InternalAppendChainValue<T>(T value)
        where T : unmanaged
    { }

    internal partial void InternalAppendChainValue<T>(T? value)
        where T : unmanaged
    { }

    internal partial void InternalAppendChainEnumValue<T>(T value)
        where T : struct, Enum
    { }

    internal partial void InternalAppendChainEnumValue<T>(T? value)
        where T : struct, Enum
    { }

    partial class InternalApi
    {
        partial class Chain
        {
            public partial void Initialize() { }

            ref partial struct ChainExecutionContext
            {
                public partial void OutputString() { }
                public partial void OutputString(string? value) { }
                public partial void OutputValueType<T>() where T : unmanaged { }
                public partial void OutputNullableValueType<T>() where T : unmanaged { }
                public partial void OutputEnum() { }
                public partial void OutputNullableEnum() { }

                public partial void SetFormatString() { }
                public partial void SetFormatString(string? format) { }

                public partial void SetKeyString() { }
                public partial void SetKeyString(string? key) { }
            }
        }
    }

#endif
}
