using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Formatting;
using JetBrains.Annotations;
using ZeroLog.Utils;

namespace ZeroLog
{
    public delegate void UnmanagedFormatterDel<T>(ref T value, StringBuffer stringBuffer, StringView view) where T : unmanaged;

    internal static unsafe class UnmanagedCache
    {
        internal delegate void FormatterDel(StringBuffer stringBuffer, byte* valuePtr, StringView view);

        private static readonly ConcurrentDictionary<IntPtr, FormatterDel> _unmanaged_structs = new ConcurrentDictionary<IntPtr, FormatterDel>();

        internal static void Register([NotNull] Type unmanagedType)
        {
            if (unmanagedType == null)
                throw new ArgumentNullException(nameof(unmanagedType));

            if (!typeof(IStringFormattable).IsAssignableFrom(unmanagedType))
                throw new ArgumentException($"Not an IStringFormattable type: {unmanagedType}");

            // Ideally we would explitly check that unmanagedType is actually unmanaged
            // However, I'm not sure that's possible.

            var generic = _register_method.MakeGenericMethod(unmanagedType);
            generic.Invoke(null, null);
        }

        private static readonly MethodInfo _register_method = typeof(UnmanagedCache).GetMethod("Register", new Type[] { });

        public static void Register<T>(UnmanagedFormatterDel<T> formatter) where T : unmanaged
        {
            var handle = typeof(T).TypeHandle.Value;
            FormatterDel raw_formatter = (b, vp, view) => FormatterGeneric<T>(b, vp, view, formatter);
            _unmanaged_structs.TryAdd(typeof(T).TypeHandle.Value, raw_formatter);
        }

        public static void Register<T>() where T : unmanaged, IStringFormattable
        {
            var handle = typeof(T).TypeHandle.Value;
            FormatterDel formatter = (b, vp, view) => FormatterGeneric<T>(b, vp, view, ValueHelper<T>.Formatter);
            _unmanaged_structs.TryAdd(typeof(T).TypeHandle.Value, formatter);
        }

        private static unsafe void FormatterGeneric<T>(StringBuffer stringBuffer, byte* valuePtr, StringView view, UnmanagedFormatterDel<T> typedFormatter) where T : unmanaged
        {
            var typedValueRef = Unsafe.AsRef<T>(valuePtr);
            typedFormatter(ref typedValueRef, stringBuffer, view);
        }

        public static bool TryGetFormatter(IntPtr typeHandle, out FormatterDel formatter)
        {
            return _unmanaged_structs.TryGetValue(typeHandle, out formatter);
        }

        // The point of this class is to allow us to generate a direct call to a known
        // method on an unknown, unconstrained generic value type. Normally this would
        // be impossible; you'd have to cast the generic argument and introduce boxing.
        // Instead we pay a one-time startup cost to create a delegate that will forward
        // the parameter to the appropriate method in a strongly typed fashion.
        static class ValueHelper<T> where T : unmanaged
        {
            public static UnmanagedFormatterDel<T> Formatter = Prepare();

            static UnmanagedFormatterDel<T> Prepare()
            {
                // we only use this class for value types that also implement IStringFormattable
                var type = typeof(T);
                if (!typeof(IStringFormattable).IsAssignableFrom(type))
                    return null;

                var result = typeof(ValueHelper<T>)
                    .GetTypeInfo()
                    .GetDeclaredMethod("Assign")
                    .MakeGenericMethod(type)
                    .Invoke(null, null);
                return (UnmanagedFormatterDel<T>)result;
            }

            public static UnmanagedFormatterDel<U> Assign<U>() where U : unmanaged, IStringFormattable
            {
                return ValueHelper2.DoFormat<U>;
            }
        }

        static class ValueHelper2
        {
            public static void DoFormat<T>(ref T input, StringBuffer buffer, StringView view) where T : unmanaged, IStringFormattable
            {
                input.Format(buffer, view);
            }
        }
    }
}
