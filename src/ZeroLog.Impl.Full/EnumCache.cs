using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using InlineIL;
using ZeroLog.Utils;
using static InlineIL.IL.Emit;

namespace ZeroLog;

internal static class EnumCache
{
    private static readonly ConcurrentDictionary<IntPtr, EnumStrings> _enums = new();
    private static readonly ConcurrentDictionary<IntPtr, bool> _isEnumSigned = new();

    public static void Register(Type enumType)
    {
        if (enumType == null)
            throw new ArgumentNullException(nameof(enumType));

        if (!enumType.IsEnum)
            throw new ArgumentException($"Not an enum type: {enumType}");

        if (enumType.ContainsGenericParameters)
            return;

        _enums.TryAdd(TypeUtil.GetTypeHandleSlow(enumType), EnumStrings.Create(enumType));
    }

    public static bool IsRegistered(Type enumType)
        => _enums.ContainsKey(TypeUtil.GetTypeHandleSlow(enumType));

    public static string? GetString(IntPtr typeHandle, ulong value, out bool registered)
    {
        if (_enums.TryGetValue(typeHandle, out var values))
        {
            registered = true;
            return values.TryGetString(value);
        }

        registered = false;
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Global")]
    public static ulong ToUInt64<T>(T value)
    {
        Ldarg(nameof(value));
        Conv_I8();
        return IL.Return<ulong>();
    }

    /// <summary>
    /// Only used when registering enums
    /// </summary>
    internal static ulong ToUInt64Slow(Enum value)
    {
        return Type.GetTypeCode(Enum.GetUnderlyingType(value.GetType())) switch
        {
            TypeCode.SByte  => ToUInt64((sbyte)(object)value),
            TypeCode.Byte   => ToUInt64((byte)(object)value),
            TypeCode.Int16  => ToUInt64((short)(object)value),
            TypeCode.UInt16 => ToUInt64((ushort)(object)value),
            TypeCode.Int32  => ToUInt64((int)(object)value),
            TypeCode.UInt32 => ToUInt64((uint)(object)value),
            TypeCode.Int64  => ToUInt64((long)(object)value),
            TypeCode.UInt64 => ToUInt64((ulong)(object)value),
            _               => throw new InvalidOperationException($"Invalid enum: {value.GetType()}")
        };
    }

    public static ulong? ToUInt64Nullable<T>(T value) // T = Nullable<SomeEnum>
    {
        return TypeUtilSlow<T>.UnderlyingTypeCode switch
        {
            TypeCode.SByte  => ToUInt64Nullable<T, sbyte>(value),
            TypeCode.Byte   => ToUInt64Nullable<T, byte>(value),
            TypeCode.Int16  => ToUInt64Nullable<T, short>(value),
            TypeCode.UInt16 => ToUInt64Nullable<T, ushort>(value),
            TypeCode.Int32  => ToUInt64Nullable<T, int>(value),
            TypeCode.UInt32 => ToUInt64Nullable<T, uint>(value),
            TypeCode.Int64  => ToUInt64Nullable<T, long>(value),
            TypeCode.UInt64 => ToUInt64Nullable<T, ulong>(value),
            _               => null
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong? ToUInt64Nullable<T, TBase>(T value) // T = Nullable<SomeEnum>
        where TBase : struct
    {
        ref var nullable = ref Unsafe.As<T, TBase?>(ref value);
        return nullable != null
            ? ToUInt64(nullable.GetValueOrDefault())
            : null;
    }

    [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
    public static bool IsEnumSigned(IntPtr typeHandle)
    {
        return _isEnumSigned.GetOrAdd(typeHandle, h => IsEnumSignedImpl(h));

        static bool IsEnumSignedImpl(IntPtr h)
        {
            var type = TypeUtil.GetTypeFromHandle(h) ?? throw new InvalidOperationException("Could not resolve type from handle");

            switch (Type.GetTypeCode(Enum.GetUnderlyingType(type)))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;

                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return false;

                default:
                    throw new InvalidOperationException($"Invalid enum: {type}");
            }
        }
    }

    private abstract class EnumStrings
    {
        public static EnumStrings Create(Type enumType)
        {
            var enumItems = Enum.GetValues(enumType)
                                .Cast<Enum>()
                                .Select(i => new EnumItem(i))
                                .ToList();

            return ArrayEnumStrings.CanHandle(enumItems)
                ? new ArrayEnumStrings(enumItems)
                : new DictionaryEnumStrings(enumItems);
        }

        public abstract string? TryGetString(ulong value);
    }

    private sealed class ArrayEnumStrings : EnumStrings
    {
        private readonly string[] _strings;

        public static bool CanHandle(IEnumerable<EnumItem> enumItems)
            => enumItems.All(i => i.Value < 32);

        public ArrayEnumStrings(List<EnumItem> enumItems)
        {
            if (enumItems.Count == 0)
            {
                _strings = Array.Empty<string>();
                return;
            }

            var maxValue = enumItems.Select(i => i.Value).Max();
            _strings = new string[maxValue + 1];

            foreach (var item in enumItems)
                _strings[item.Value] = item.Name;
        }

        public override string? TryGetString(ulong value)
            => value < (ulong)_strings.Length
                ? _strings[unchecked((int)value)]
                : null;
    }

    private sealed class DictionaryEnumStrings : EnumStrings
    {
        private readonly Dictionary<ulong, string> _strings = new();

        public DictionaryEnumStrings(List<EnumItem> enumItems)
        {
            foreach (var item in enumItems)
                _strings[item.Value] = item.Name;
        }

        public override string? TryGetString(ulong value)
        {
            _strings.TryGetValue(value, out var str);
            return str;
        }
    }

    private struct EnumItem
    {
        public ulong Value { get; }
        public string Name { get; }

        public EnumItem(Enum item)
        {
            Value = ToUInt64Slow(item);
            Name = item.ToString();
        }
    }
}
