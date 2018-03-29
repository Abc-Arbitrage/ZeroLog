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

            _enums.TryAdd(TypeUtil.GetTypeHandleSlow(enumType), EnumStrings.Create(enumType));
        }

        public static bool IsRegistered([NotNull] Type enumType)
            => _enums.ContainsKey(TypeUtil.GetTypeHandleSlow(enumType));

        public static string TryGetString(IntPtr typeHandle, ulong value)
            => _enums.TryGetValue(typeHandle, out var values)
                ? values.TryGetString(value)
                : null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ToUInt64<T>(T value)
        {
            IL.Push(value);
            IL.Emit(OpCodes.Conv_I8);
            return IL.Return<ulong>();
        }

        private static ulong ToUInt64Slow(Enum value)
        {
            unchecked
            {
                switch (Type.GetTypeCode(Enum.GetUnderlyingType(value.GetType())))
                {
                    case TypeCode.Byte:
                        return (byte)(object)value;

                    case TypeCode.SByte:
                        return (ulong)(sbyte)(object)value;

                    case TypeCode.Int16:
                        return (ulong)(short)(object)value;

                    case TypeCode.UInt16:
                        return (ushort)(object)value;

                    case TypeCode.Int32:
                        return (ulong)(int)(object)value;

                    case TypeCode.UInt32:
                        return (uint)(object)value;

                    case TypeCode.Int64:
                        return (ulong)(long)(object)value;

                    case TypeCode.UInt64:
                        return (ulong)(object)value;

                    default:
                        throw new InvalidOperationException($"Invalid enum: {value.GetType()}");
                }
            }
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
                    _strings = Array.Empty<string>();
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
