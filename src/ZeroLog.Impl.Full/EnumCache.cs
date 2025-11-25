using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using InlineIL;
using ZeroLog.Support;
using static InlineIL.IL.Emit;

namespace ZeroLog;

internal static class EnumCache
{
    private static readonly ConcurrentDictionary<IntPtr, EnumStrings> _enums = new();
    private static readonly ConcurrentDictionary<IntPtr, bool> _isEnumSigned = new();

    public static void Register([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type enumType)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
            throw new ArgumentException($"Not an enum type: {enumType}");

        if (enumType.ContainsGenericParameters)
            return;

        _enums.TryAdd(TypeUtil.GetTypeHandleSlow(enumType), EnumStrings.Create(enumType));
    }

    public static void Register<TEnum>()
        where TEnum : struct, Enum
        => _enums.TryAdd(TypeUtil.GetTypeHandleSlow(typeof(TEnum)), EnumStrings.Create<TEnum>());

    public static void Remove(Type enumType)
        => _enums.TryRemove(TypeUtil.GetTypeHandleSlow(enumType), out _);

    public static void Ignore(Type enumType)
        => _enums[TypeUtil.GetTypeHandleSlow(enumType)] = NullEnumStrings.Instance;

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

    [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Global")]
    public static T FromUInt64<T>(ulong value)
        where T : struct
    {
        switch (TypeUtilSlow<T>.UnderlyingEnumTypeCode)
        {
            case TypeCode.SByte:
                Ldarg(nameof(value));
                Conv_I1();
                return IL.Return<T>();

            case TypeCode.Byte:
                Ldarg(nameof(value));
                Conv_U1();
                return IL.Return<T>();

            case TypeCode.Int16:
                Ldarg(nameof(value));
                Conv_I2();
                return IL.Return<T>();

            case TypeCode.UInt16:
                Ldarg(nameof(value));
                Conv_U2();
                return IL.Return<T>();

            case TypeCode.Int32:
                Ldarg(nameof(value));
                Conv_I4();
                return IL.Return<T>();

            case TypeCode.UInt32:
                Ldarg(nameof(value));
                Conv_U4();
                return IL.Return<T>();

            case TypeCode.Int64:
                Ldarg(nameof(value));
                Conv_I8();
                return IL.Return<T>();

            case TypeCode.UInt64:
                Ldarg(nameof(value));
                Conv_U8();
                return IL.Return<T>();

            default:
                return default;
        }
    }

    internal static ulong ToUInt64Slow(Enum value)
    {
        // Only used when registering enums.

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

    private static ulong ToUInt64Unbox(object? value)
    {
        // Only used when registering enums.

        return value switch
        {
            sbyte i  => ToUInt64(i),
            byte i   => ToUInt64(i),
            short i  => ToUInt64(i),
            ushort i => ToUInt64(i),
            int i    => ToUInt64(i),
            uint i   => ToUInt64(i),
            long i   => ToUInt64(i),
            ulong i  => ToUInt64(i),
            _        => throw new InvalidOperationException($"Invalid enum: {value?.GetType()}")
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
        return _isEnumSigned.GetOrAdd(typeHandle, static h => IsEnumSignedImpl(h));

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
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL3050", Justification = "Check done manually")]
        public static EnumStrings Create([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type enumType)
            => Create(
                RuntimeFeature.IsDynamicCodeSupported
                    ? Enum.GetValues(enumType).Cast<Enum>().Select(i => new EnumItem(i))
                    : enumType.GetFields(BindingFlags.Public | BindingFlags.Static).Select(i => new EnumItem(i))
            );

        public static EnumStrings Create<TEnum>()
            where TEnum : struct, Enum
            => Create(Enum.GetValues<TEnum>().Select(i => new EnumItem(i)));

        private static EnumStrings Create(IEnumerable<EnumItem> enumItems)
        {
            var itemList = enumItems.ToList();

            if (itemList.Count == 0)
                return NullEnumStrings.Instance;

            return ArrayEnumStrings.CanHandle(itemList)
                ? new ArrayEnumStrings(itemList)
                : new DictionaryEnumStrings(itemList);
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
                _strings = [];
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

    private sealed class NullEnumStrings : EnumStrings
    {
        public static NullEnumStrings Instance { get; } = new();

        public override string? TryGetString(ulong value)
            => null;
    }

    private readonly struct EnumItem
    {
        public string Name { get; }
        public ulong Value { get; }

        public EnumItem(Enum item)
        {
            Name = item.ToString();
            Value = ToUInt64Slow(item);
        }

        public EnumItem(FieldInfo field)
        {
            Name = field.Name;
            Value = ToUInt64Unbox(field.GetRawConstantValue());
        }
    }
}
