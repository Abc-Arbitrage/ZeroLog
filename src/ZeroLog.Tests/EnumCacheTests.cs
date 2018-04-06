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
    }
}
