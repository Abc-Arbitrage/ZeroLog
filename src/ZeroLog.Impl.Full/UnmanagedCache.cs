using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using ZeroLog.Utils;

namespace ZeroLog;

public delegate bool UnmanagedFormatterDelegate<T>(
    ref T value,
    Span<char> destination,
    out int charsWritten,
    ReadOnlySpan<char> format
)
    where T : unmanaged;

internal static unsafe class UnmanagedCache
{
    internal delegate bool FormatterDelegate(
        byte* valuePtr,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format
    );

    private static readonly Dictionary<IntPtr, FormatterDelegate> _unmanagedStructs = new();
    private static readonly MethodInfo _registerMethod = typeof(UnmanagedCache).GetMethod(nameof(Register), Type.EmptyTypes)!;

    internal static void Register(Type unmanagedType)
    {
        if (unmanagedType == null)
            throw new ArgumentNullException(nameof(unmanagedType));

        if (!typeof(ISpanFormattable).IsAssignableFrom(unmanagedType))
            throw new ArgumentException($"Not an {nameof(ISpanFormattable)} type: {unmanagedType}");

        if (!TypeUtil.GetIsUnmanagedSlow(unmanagedType))
            throw new ArgumentException($"Not an unmanaged type: {unmanagedType}");

        _registerMethod.MakeGenericMethod(unmanagedType).Invoke(null, null);
    }

    public static void Register<T>(UnmanagedFormatterDelegate<T> formatter)
        where T : unmanaged
    {
        lock (_unmanagedStructs)
        {
            _unmanagedStructs[typeof(T).TypeHandle.Value] = (byte* valuePtr, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format)
                => FormatterGeneric(valuePtr, destination, out charsWritten, format, formatter);

            _unmanagedStructs[typeof(T?).TypeHandle.Value] = (byte* valuePtr, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format)
                => FormatterGenericNullable(valuePtr, destination, out charsWritten, format, formatter);
        }
    }

    [SuppressMessage("ReSharper", "ConvertToLocalFunction")]
    public static void Register<T>()
        where T : unmanaged, ISpanFormattable
    {
        UnmanagedFormatterDelegate<T> formatter = static (ref T input, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format)
            => input.TryFormat(destination, out charsWritten, format, CultureInfo.InvariantCulture);

        Register(formatter);
    }

    private static bool FormatterGeneric<T>(byte* valuePtr, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, UnmanagedFormatterDelegate<T>? typedFormatter)
        where T : unmanaged
    {
        if (typedFormatter != null)
            return typedFormatter.Invoke(ref Unsafe.AsRef<T>(valuePtr), destination, out charsWritten, format);

        charsWritten = 0;
        return true;
    }

    private static bool FormatterGenericNullable<T>(byte* valuePtr, Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, UnmanagedFormatterDelegate<T>? typedFormatter)
        where T : unmanaged
    {
        ref var typedValueRef = ref Unsafe.AsRef<T?>(valuePtr);

        if (typedValueRef != null)
        {
            var value = typedValueRef.GetValueOrDefault(); // This copies the value, but this is the slower execution path anyway.

            if (typedFormatter != null)
                return typedFormatter.Invoke(ref value, destination, out charsWritten, format);

            charsWritten = 0;
            return true;
        }
        else
        {
            var value = LogManager.Config.NullDisplayString;
            charsWritten = 0;

            if (value.TryCopyTo(destination))
                charsWritten = value.Length;

            return true;
        }
    }

    public static bool TryGetFormatter(IntPtr typeHandle, out FormatterDelegate formatter)
    {
        // This is accessed from a single thread, there should be no contention
        lock (_unmanagedStructs)
        {
            return _unmanagedStructs.TryGetValue(typeHandle, out formatter!);
        }
    }
}
