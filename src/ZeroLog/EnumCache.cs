using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using InlineIL;
using JetBrains.Annotations;
using ZeroLog.Utils;

namespace ZeroLog
{
    internal static class EnumCache
    {
        private static readonly ConcurrentDictionary<IntPtr, EnumStrings> _enums = new ConcurrentDictionary<IntPtr, EnumStrings>();
        private static readonly ConcurrentDictionary<IntPtr, bool> _isEnumSigned = new ConcurrentDictionary<IntPtr, bool>();

        public static void Register([NotNull] Type enumType)
        {
            if (enumType == null)
                throw new ArgumentNullException(nameof(enumType));

            if (!enumType.IsEnum)
                throw new ArgumentException($"Not an enum type: {enumType}");

            if (enumType.IsNested)
            {
                var declaringType = enumType.DeclaringType;

                while (declaringType != null)
                {
                    if (declaringType.IsGenericType)
                        return;

                    declaringType = declaringType.DeclaringType;
                }
            }

            _enums.TryAdd(TypeUtil.GetTypeHandleSlow(enumType), EnumStrings.Create(enumType));
        }

        public static bool IsRegistered([NotNull] Type enumType)
            => _enums.ContainsKey(TypeUtil.GetTypeHandleSlow(enumType));

        [CanBeNull]
        public static string GetString(IntPtr typeHandle, ulong value, out bool registered)
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
        public static ulong ToUInt64<T>(T value)
        {
            IL.Push(value);
            IL.Emit(OpCodes.Conv_I8);
            return IL.Return<ulong>();
        }

        /// <summary>
        /// Only used when registering enums
        /// </summary>
        internal static ulong ToUInt64Slow(Enum value)
        {
            switch (Type.GetTypeCode(Enum.GetUnderlyingType(value.GetType())))
            {
                case TypeCode.SByte:
                    return ToUInt64((sbyte)(object)value);

                case TypeCode.Byte:
                    return ToUInt64((byte)(object)value);

                case TypeCode.Int16:
                    return ToUInt64((short)(object)value);

                case TypeCode.UInt16:
                    return ToUInt64((ushort)(object)value);

                case TypeCode.Int32:
                    return ToUInt64((int)(object)value);

                case TypeCode.UInt32:
                    return ToUInt64((uint)(object)value);

                case TypeCode.Int64:
                    return ToUInt64((long)(object)value);

                case TypeCode.UInt64:
                    return ToUInt64((ulong)(object)value);

                default:
                    throw new InvalidOperationException($"Invalid enum: {value.GetType()}");
            }
        }

        public static ulong? ToUInt64Nullable<T>(T value) // T = Nullable<SomeEnum>
        {
            switch (TypeUtilNullable<T>.UnderlyingTypeCode)
            {
                case TypeCode.SByte:
                    return ToUInt64Nullable<T, sbyte>(value);

                case TypeCode.Byte:
                    return ToUInt64Nullable<T, byte>(value);

                case TypeCode.Int16:
                    return ToUInt64Nullable<T, short>(value);

                case TypeCode.UInt16:
                    return ToUInt64Nullable<T, ushort>(value);

                case TypeCode.Int32:
                    return ToUInt64Nullable<T, int>(value);

                case TypeCode.UInt32:
                    return ToUInt64Nullable<T, uint>(value);

                case TypeCode.Int64:
                    return ToUInt64Nullable<T, long>(value);

                case TypeCode.UInt64:
                    return ToUInt64Nullable<T, ulong>(value);

                default:
                    return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong? ToUInt64Nullable<T, TBase>(T value) // T = Nullable<SomeEnum>
            where TBase : struct
        {
            ref var nullable = ref TypeUtil.As<T, TBase?>(ref value);
            return nullable != null
                ? ToUInt64(nullable.GetValueOrDefault())
                : (ulong?)null;
        }

        [SuppressMessage("ReSharper", "ConvertClosureToMethodGroup")]
        public static bool IsEnumSigned(IntPtr typeHandle)
        {
            return _isEnumSigned.GetOrAdd(typeHandle, h => IsEnumSignedImpl(h));

            bool IsEnumSignedImpl(IntPtr h)
            {
                var type = TypeUtil.GetTypeFromHandle(h);
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
                    ? (EnumStrings)new ArrayEnumStrings(enumItems)
                    : new DictionaryEnumStrings(enumItems);
            }

            [CanBeNull]
            public abstract string TryGetString(ulong value);
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
                    _strings = ArrayUtil.Empty<string>();
                    return;
                }

                var maxValue = enumItems.Select(i => i.Value).Max();
                _strings = new string[maxValue + 1];

                foreach (var item in enumItems)
                    _strings[item.Value] = item.Name;
            }

            public override string TryGetString(ulong value)
                => value < (ulong)_strings.Length
                    ? _strings[unchecked((int)value)]
                    : null;
        }

        private sealed class DictionaryEnumStrings : EnumStrings
        {
            private readonly Dictionary<ulong, string> _strings = new Dictionary<ulong, string>();

            public DictionaryEnumStrings(List<EnumItem> enumItems)
            {
                foreach (var item in enumItems)
                    _strings[item.Value] = item.Name;
            }

            public override string TryGetString(ulong value)
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
}
