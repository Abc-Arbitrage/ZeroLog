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
            Check.That(GetString(EnumByte.Foo)).Equals("Foo");
            Check.That(GetString(EnumByte.Bar)).Equals("Bar");
            Check.That(GetString(EnumByte.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_sbyte()
        {
            Check.That(GetString(EnumSByte.Foo)).Equals("Foo");
            Check.That(GetString(EnumSByte.Bar)).Equals("Bar");
            Check.That(GetString(EnumSByte.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_int16()
        {
            Check.That(GetString(EnumInt16.Foo)).Equals("Foo");
            Check.That(GetString(EnumInt16.Bar)).Equals("Bar");
            Check.That(GetString(EnumInt16.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint16()
        {
            Check.That(GetString(EnumUInt16.Foo)).Equals("Foo");
            Check.That(GetString(EnumUInt16.Bar)).Equals("Bar");
            Check.That(GetString(EnumUInt16.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_int32()
        {
            Check.That(GetString(EnumInt32.Foo)).Equals("Foo");
            Check.That(GetString(EnumInt32.Bar)).Equals("Bar");
            Check.That(GetString(EnumInt32.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint32()
        {
            Check.That(GetString(EnumUInt32.Foo)).Equals("Foo");
            Check.That(GetString(EnumUInt32.Bar)).Equals("Bar");
            Check.That(GetString(EnumUInt32.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_int64()
        {
            Check.That(GetString(EnumInt64.Foo)).Equals("Foo");
            Check.That(GetString(EnumInt64.Bar)).Equals("Bar");
            Check.That(GetString(EnumInt64.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_uint64()
        {
            Check.That(GetString(EnumUInt64.Foo)).Equals("Foo");
            Check.That(GetString(EnumUInt64.Bar)).Equals("Bar");
            Check.That(GetString(EnumUInt64.Baz)).Equals("Baz");
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
            Check.That(GetString(EnumWithNegativeValues.Foo)).Equals("Foo");
            Check.That(GetString(EnumWithNegativeValues.Bar)).Equals("Bar");
            Check.That(GetString(EnumWithNegativeValues.Baz)).Equals("Baz");
        }

        [Test]
        public void should_get_enum_strings_on_enums_with_large_values()
        {
            Check.That(GetString(EnumWithLargeValues.Foo)).Equals("Foo");
            Check.That(GetString(EnumWithLargeValues.Bar)).Equals("Bar");
            Check.That(GetString(EnumWithLargeValues.Baz)).Equals("Baz");
        }

        [Test]
        public void should_return_sign_info()
        {
            Check.That(GetIsSigned<EnumByte>()).Equals(false);
            Check.That(GetIsSigned<EnumSByte>()).Equals(true);
            Check.That(GetIsSigned<EnumInt16>()).Equals(true);
            Check.That(GetIsSigned<EnumUInt16>()).Equals(false);
            Check.That(GetIsSigned<EnumInt32>()).Equals(true);
            Check.That(GetIsSigned<EnumUInt32>()).Equals(false);
            Check.That(GetIsSigned<EnumInt64>()).Equals(true);
            Check.That(GetIsSigned<EnumUInt64>()).Equals(false);
        }

        private static string GetString<T>(T value)
            where T : struct
        {
            EnumCache.Register(typeof(T));
            return EnumCache.TryGetString(TypeUtil.GetTypeHandle<T>(), EnumCache.ToUInt64(value));
        }

        private static bool GetIsSigned<T>()
            where T : struct
        {
            return EnumCache.IsEnumSigned(TypeUtil.GetTypeHandle<T>());
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
    }
}
