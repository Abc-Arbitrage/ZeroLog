using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Formatting;
using JetBrains.Annotations;
using ZeroLog.Utils;

namespace ZeroLog
{
    public delegate void UnmanagedFormatterDelegate<T>(ref T value, StringBuffer stringBuffer, StringView view)
        where T : unmanaged;

    internal static unsafe class UnmanagedCache
    {
        internal delegate void FormatterDelegate(StringBuffer stringBuffer, byte* valuePtr, StringView view);

        private static readonly Dictionary<IntPtr, FormatterDelegate> _unmanagedStructs = new Dictionary<IntPtr, FormatterDelegate>();
        private static readonly MethodInfo _registerMethod = typeof(UnmanagedCache).GetMethod(nameof(Register), Type.EmptyTypes);

        internal static void Register([NotNull] Type unmanagedType)
        {
            if (unmanagedType == null)
                throw new ArgumentNullException(nameof(unmanagedType));

            if (!typeof(IStringFormattable).IsAssignableFrom(unmanagedType))
                throw new ArgumentException($"Not an {nameof(IStringFormattable)} type: {unmanagedType}");

            if (!TypeUtil.GetIsUnmanagedSlow(unmanagedType))
                throw new ArgumentException($"Not an unmanaged type: {unmanagedType}");

            _registerMethod.MakeGenericMethod(unmanagedType).Invoke(null, null);
        }

        public static void Register<T>(UnmanagedFormatterDelegate<T> formatter)
            where T : unmanaged
        {
            lock (_unmanagedStructs)
            {
                _unmanagedStructs[typeof(T).TypeHandle.Value] = (b, vp, view) => FormatterGeneric(b, vp, view, formatter);
                _unmanagedStructs[typeof(T?).TypeHandle.Value] = (b, vp, view) => FormatterGenericNullable(b, vp, view, formatter);
            }
        }

        [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
        public static void Register<T>()
            where T : unmanaged, IStringFormattable
        {
            UnmanagedFormatterDelegate<T> formatter = (ref T input, StringBuffer buffer, StringView view) => input.Format(buffer, view);

            lock (_unmanagedStructs)
            {
                _unmanagedStructs[typeof(T).TypeHandle.Value] = (b, vp, view) => FormatterGeneric(b, vp, view, formatter);
                _unmanagedStructs[typeof(T?).TypeHandle.Value] = (b, vp, view) => FormatterGenericNullable(b, vp, view, formatter);
            }
        }

        private static void FormatterGeneric<T>(StringBuffer stringBuffer, byte* valuePtr, StringView view, UnmanagedFormatterDelegate<T> typedFormatter)
            where T : unmanaged
        {
            typedFormatter?.Invoke(ref Unsafe.AsRef<T>(valuePtr), stringBuffer, view);
        }

        private static void FormatterGenericNullable<T>(StringBuffer stringBuffer, byte* valuePtr, StringView view, UnmanagedFormatterDelegate<T> typedFormatter)
            where T : unmanaged
        {
            ref var typedValueRef = ref Unsafe.AsRef<T?>(valuePtr);

            if (typedValueRef != null)
            {
                var value = typedValueRef.GetValueOrDefault(); // This copies the value, but this is the slower execution path anyway.
                typedFormatter?.Invoke(ref value, stringBuffer, view);
            }
            else
            {
                stringBuffer.Append(LogManager.Config.NullDisplayString);
            }
        }

        public static bool TryGetFormatter(IntPtr typeHandle, out FormatterDelegate formatter)
        {
            // This is accessed from a single thread, there should be no contention
            lock (_unmanagedStructs)
            {
                return _unmanagedStructs.TryGetValue(typeHandle, out formatter);
            }
        }
    }
}
