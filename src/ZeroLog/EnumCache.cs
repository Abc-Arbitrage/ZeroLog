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
                var enumValues = Enum.GetValues(enumType)
                                     .Cast<Enum>()
                                     .ToList();

                return ArrayEnumStrings.CanHandle(enumValues)
                    ? (EnumStrings)new ArrayEnumStrings(enumValues)
                    : new DictionaryEnumStrings(enumValues);
            }

            [CanBeNull]
            public abstract string TryGetString(ulong value);
        }

        private sealed class ArrayEnumStrings : EnumStrings
        {
            private readonly string[] _strings;

            public static bool CanHandle(IEnumerable<Enum> enumValues)
                => enumValues.All(i => ToUInt64Slow(i) <= 32);

            public ArrayEnumStrings(List<Enum> enumValues)
            {
                if (enumValues.Count == 0)
                {
                    _strings = Array.Empty<string>();
                    return;
                }

                var maxValue = (int)enumValues.Select(ToUInt64Slow).Max();
                _strings = new string[maxValue + 1];

                foreach (var value in enumValues)
                    _strings[ToUInt64Slow(value)] = value.ToString();
            }

            public override string TryGetString(ulong value)
                => value < (ulong)_strings.Length
                    ? _strings[unchecked((int)value)]
                    : null;
        }

        private sealed class DictionaryEnumStrings : EnumStrings
        {
            private readonly Dictionary<ulong, string> _strings = new Dictionary<ulong, string>();

            public DictionaryEnumStrings(List<Enum> enumValues)
            {
                foreach (var value in enumValues)
                    _strings[ToUInt64Slow(value)] = value.ToString();
            }

            public override string TryGetString(ulong value)
            {
                _strings.TryGetValue(value, out var str);
                return str;
            }
        }
    }
}
