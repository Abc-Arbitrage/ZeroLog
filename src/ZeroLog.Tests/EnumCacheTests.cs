using System;
using System.Diagnostics.CodeAnalysis;
using NFluent;
using NUnit.Framework;
using ZeroLog.Utils;

namespace ZeroLog.Tests
{
    public class EnumCacheTests
    {
        [Test]
        public void should_get_enum_strings_byte()
        {
            Check.That(GetString(EnumByte.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumByte.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumByte.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_sbyte()
        {
            Check.That(GetString(EnumSByte.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumSByte.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumSByte.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_int16()
        {
            Check.That(GetString(EnumInt16.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumInt16.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumInt16.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint16()
        {
            Check.That(GetString(EnumUInt16.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumUInt16.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumUInt16.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_int32()
        {
            Check.That(GetString(EnumInt32.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumInt32.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumInt32.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint32()
        {
            Check.That(GetString(EnumUInt32.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumUInt32.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumUInt32.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_int64()
        {
            Check.That(GetString(EnumInt64.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumInt64.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumInt64.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint64()
        {
            Check.That(GetString(EnumUInt64.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumUInt64.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumUInt64.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_return_null_on_unknown_value()
        {
            Check.That(GetString((EnumInt32)42)).IsNull();
            Check.That(GetString((EnumInt32)(-42))).IsNull();

            Check.That(GetString((EnumWithNegativeValues)42)).IsNull();
            Check.That(GetString((EnumWithNegativeValues)(-42))).IsNull();
        }

        [Test]
        public void should_get_enum_strings_on_enums_with_negative_values()
        {
            Check.That(GetString(EnumWithNegativeValues.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumWithNegativeValues.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumWithNegativeValues.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_on_enums_with_large_values()
        {
            Check.That(GetString(EnumWithLargeValues.Foo)).IsEqualTo("Foo");
            Check.That(GetString(EnumWithLargeValues.Bar)).IsEqualTo("Bar");
            Check.That(GetString(EnumWithLargeValues.Baz)).IsEqualTo("Baz");
        }

        [Test]
        public void should_get_enum_strings_on_enums_with_value_larger_than_item_count()
        {
            Check.That(GetString(EnumWithValueLargerThanItemCount.Foo)).IsEqualTo("Foo");
        }

        [Test]
        public void should_get_enum_strings_on_empty_enums()
        {
            Check.That(GetString(default(EmptyEnum))).IsNull();
        }

        [Test]
        public void should_return_sign_info()
        {
            Check.That(GetIsSigned<EnumByte>()).IsFalse();
            Check.That(GetIsSigned<EnumSByte>()).IsTrue();
            Check.That(GetIsSigned<EnumInt16>()).IsTrue();
            Check.That(GetIsSigned<EnumUInt16>()).IsFalse();
            Check.That(GetIsSigned<EnumInt32>()).IsTrue();
            Check.That(GetIsSigned<EnumUInt32>()).IsFalse();
            Check.That(GetIsSigned<EnumInt64>()).IsTrue();
            Check.That(GetIsSigned<EnumUInt64>()).IsFalse();
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_byte()
        {
            CheckNullableValue(EnumMinMaxByte.Min);
            CheckNullableValue(EnumMinMaxByte.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_sbyte()
        {
            CheckNullableValue(EnumMinMaxSByte.Min);
            CheckNullableValue(EnumMinMaxSByte.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_int16()
        {
            CheckNullableValue(EnumMinMaxInt16.Min);
            CheckNullableValue(EnumMinMaxInt16.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_uint16()
        {
            CheckNullableValue(EnumMinMaxUInt16.Min);
            CheckNullableValue(EnumMinMaxUInt16.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_int32()
        {
            CheckNullableValue(EnumMinMaxInt32.Min);
            CheckNullableValue(EnumMinMaxInt32.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_uint32()
        {
            CheckNullableValue(EnumMinMaxUInt32.Min);
            CheckNullableValue(EnumMinMaxUInt32.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_int64()
        {
            CheckNullableValue(EnumMinMaxInt64.Min);
            CheckNullableValue(EnumMinMaxInt64.Max);
        }

        [Test]
        public void should_correctly_handle_nullable_values_for_uint64()
        {
            CheckNullableValue(EnumMinMaxUInt64.Min);
            CheckNullableValue(EnumMinMaxUInt64.Max);
        }

        [Test]
        public void should_not_throw_when_registering_enum_in_open_type()
        {
            EnumCache.Register(typeof(GenericType<>.EnumInGenericType));
            EnumCache.Register(typeof(GenericType<>.AnotherOne<>.EnumInGenericType2));
        }

        private static string GetString<T>(T value)
            where T : struct
        {
            EnumCache.Register(typeof(T));
            return EnumCache.GetString(TypeUtil<T>.TypeHandle, EnumCache.ToUInt64(value), out _);
        }

        private static bool GetIsSigned<T>()
            where T : struct
        {
            return EnumCache.IsEnumSigned(TypeUtil<T>.TypeHandle);
        }

        private static void CheckNullableValue<T>(T value)
            where T : struct
        {
            Check.That(EnumCache.ToUInt64Slow((Enum)(object)value)).IsEqualTo(EnumCache.ToUInt64(value));
            Check.That(EnumCache.ToUInt64Nullable((T?)value)).IsEqualTo(EnumCache.ToUInt64(value));
            Check.That(GetString(value)).IsEqualTo(value.ToString());
        }

        private enum EnumByte : byte
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumSByte : sbyte
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumInt16 : short
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumUInt16 : ushort
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumInt32
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumUInt32 : uint
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumInt64 : long
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumUInt64 : ulong
        {
            Foo,
            Bar,
            Baz
        }

        private enum EnumWithNegativeValues
        {
            Foo = -1,
            Bar = 1,
            Baz
        }

        private enum EnumWithLargeValues : long
        {
            Foo = long.MinValue,
            Bar = 0,
            Baz = long.MaxValue
        }

        private enum EnumWithValueLargerThanItemCount
        {
            Foo = 4
        }

        private enum EmptyEnum
        {
        }

        private enum EnumMinMaxByte : byte
        {
            Min = byte.MinValue,
            Max = byte.MaxValue
        }

        private enum EnumMinMaxSByte : sbyte
        {
            Min = sbyte.MinValue,
            Max = sbyte.MaxValue
        }

        private enum EnumMinMaxInt16 : short
        {
            Min = short.MinValue,
            Max = short.MaxValue
        }

        private enum EnumMinMaxUInt16 : ushort
        {
            Min = ushort.MinValue,
            Max = ushort.MaxValue
        }

        private enum EnumMinMaxInt32
        {
            Min = int.MinValue,
            Max = int.MaxValue
        }

        private enum EnumMinMaxUInt32 : uint
        {
            Min = uint.MinValue,
            Max = uint.MaxValue
        }

        private enum EnumMinMaxInt64 : long
        {
            Min = long.MinValue,
            Max = long.MaxValue
        }

        private enum EnumMinMaxUInt64 : ulong
        {
            Min = ulong.MinValue,
            Max = ulong.MaxValue
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
        private class GenericType<TFoo>
        {
            public enum EnumInGenericType
            {
            }

            public class AnotherOne<TBar>
            {
                public enum EnumInGenericType2
                {
                }
            }
        }
    }
}
