using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Shouldly;
using ZeroLog.Support;

namespace ZeroLog.Tests;

public class EnumCacheTests
{
    [Test]
    public void should_get_enum_strings_byte()
    {
        GetString(EnumByte.Foo).ShouldBe("Foo");
        GetString(EnumByte.Bar).ShouldBe("Bar");
        GetString(EnumByte.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_sbyte()
    {
        GetString(EnumSByte.Foo).ShouldBe("Foo");
        GetString(EnumSByte.Bar).ShouldBe("Bar");
        GetString(EnumSByte.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_int16()
    {
        GetString(EnumInt16.Foo).ShouldBe("Foo");
        GetString(EnumInt16.Bar).ShouldBe("Bar");
        GetString(EnumInt16.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_uint16()
    {
        GetString(EnumUInt16.Foo).ShouldBe("Foo");
        GetString(EnumUInt16.Bar).ShouldBe("Bar");
        GetString(EnumUInt16.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_int32()
    {
        GetString(EnumInt32.Foo).ShouldBe("Foo");
        GetString(EnumInt32.Bar).ShouldBe("Bar");
        GetString(EnumInt32.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_uint32()
    {
        GetString(EnumUInt32.Foo).ShouldBe("Foo");
        GetString(EnumUInt32.Bar).ShouldBe("Bar");
        GetString(EnumUInt32.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_int64()
    {
        GetString(EnumInt64.Foo).ShouldBe("Foo");
        GetString(EnumInt64.Bar).ShouldBe("Bar");
        GetString(EnumInt64.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_uint64()
    {
        GetString(EnumUInt64.Foo).ShouldBe("Foo");
        GetString(EnumUInt64.Bar).ShouldBe("Bar");
        GetString(EnumUInt64.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_round_trip_value_byte()
    {
        var value = EnumCache.ToUInt64(EnumByte.Baz);
        EnumCache.FromUInt64<EnumByte>(value).ShouldBe(EnumByte.Baz);
    }

    [Test]
    public void should_round_trip_value_sbyte()
    {
        var value = EnumCache.ToUInt64(EnumSByte.Baz);
        EnumCache.FromUInt64<EnumSByte>(value).ShouldBe(EnumSByte.Baz);
    }

    [Test]
    public void should_round_trip_value_int16()
    {
        var value = EnumCache.ToUInt64(EnumInt16.Baz);
        EnumCache.FromUInt64<EnumInt16>(value).ShouldBe(EnumInt16.Baz);
    }

    [Test]
    public void should_round_trip_value_uint16()
    {
        var value = EnumCache.ToUInt64(EnumUInt16.Baz);
        EnumCache.FromUInt64<EnumUInt16>(value).ShouldBe(EnumUInt16.Baz);
    }

    [Test]
    public void should_round_trip_value_int32()
    {
        var value = EnumCache.ToUInt64(EnumInt32.Baz);
        EnumCache.FromUInt64<EnumInt32>(value).ShouldBe(EnumInt32.Baz);
    }

    [Test]
    public void should_round_trip_value_uint32()
    {
        var value = EnumCache.ToUInt64(EnumUInt32.Baz);
        EnumCache.FromUInt64<EnumUInt32>(value).ShouldBe(EnumUInt32.Baz);
    }

    [Test]
    public void should_round_trip_value_int64()
    {
        var value = EnumCache.ToUInt64(EnumInt64.Baz);
        EnumCache.FromUInt64<EnumInt64>(value).ShouldBe(EnumInt64.Baz);
    }

    [Test]
    public void should_round_trip_value_uint64()
    {
        var value = EnumCache.ToUInt64(EnumUInt64.Baz);
        EnumCache.FromUInt64<EnumUInt64>(value).ShouldBe(EnumUInt64.Baz);
    }

    [Test]
    public void should_round_trip_negative_value()
    {
        var value = EnumCache.ToUInt64(EnumWithNegativeValues.Foo);
        EnumCache.FromUInt64<EnumWithNegativeValues>(value).ShouldBe(EnumWithNegativeValues.Foo);
    }

    [Test]
    public void should_return_null_on_unknown_value()
    {
        GetString((EnumInt32)42).ShouldBeNull();
        GetString((EnumInt32)(-42)).ShouldBeNull();

        GetString((EnumWithNegativeValues)42).ShouldBeNull();
        GetString((EnumWithNegativeValues)(-42)).ShouldBeNull();
    }

    [Test]
    public void should_get_enum_strings_on_enums_with_negative_values()
    {
        GetString(EnumWithNegativeValues.Foo).ShouldBe("Foo");
        GetString(EnumWithNegativeValues.Bar).ShouldBe("Bar");
        GetString(EnumWithNegativeValues.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_on_enums_with_large_values()
    {
        GetString(EnumWithLargeValues.Foo).ShouldBe("Foo");
        GetString(EnumWithLargeValues.Bar).ShouldBe("Bar");
        GetString(EnumWithLargeValues.Baz).ShouldBe("Baz");
    }

    [Test]
    public void should_get_enum_strings_on_enums_with_value_larger_than_item_count()
    {
        GetString(EnumWithValueLargerThanItemCount.Foo).ShouldBe("Foo");
    }

    [Test]
    public void should_get_enum_strings_on_empty_enums()
    {
        GetString(default(EmptyEnum)).ShouldBeNull();
    }

    [Test]
    public void should_return_sign_info()
    {
        GetIsSigned<EnumByte>().ShouldBeFalse();
        GetIsSigned<EnumSByte>().ShouldBeTrue();
        GetIsSigned<EnumInt16>().ShouldBeTrue();
        GetIsSigned<EnumUInt16>().ShouldBeFalse();
        GetIsSigned<EnumInt32>().ShouldBeTrue();
        GetIsSigned<EnumUInt32>().ShouldBeFalse();
        GetIsSigned<EnumInt64>().ShouldBeTrue();
        GetIsSigned<EnumUInt64>().ShouldBeFalse();
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

    [Test]
    public void should_handle_nested_enums_in_constructed_generic_types()
    {
        GetString(GenericType<int>.EnumInGenericType.Foo).ShouldBe("Foo");
        GetString(GenericType<int>.AnotherOne<string>.EnumInGenericType2.Foo).ShouldBe("Foo");
    }

    [Test]
    public void should_ignore_enum()
    {
        EnumCache.Register(typeof(EnumToIgnore));
        EnumCache.GetString(typeof(EnumToIgnore).TypeHandle.Value, (ulong)EnumToIgnore.Bar, out _)
                 .ShouldBe(nameof(EnumToIgnore.Bar));

        EnumCache.Ignore(typeof(EnumToIgnore));
        EnumCache.GetString(typeof(EnumToIgnore).TypeHandle.Value, (ulong)EnumToIgnore.Bar, out _)
                 .ShouldBeNull();
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
        EnumCache.ToUInt64Slow((Enum)(object)value).ShouldBe(EnumCache.ToUInt64(value));
        EnumCache.ToUInt64Nullable((T?)value).ShouldBe(EnumCache.ToUInt64(value));
        GetString(value).ShouldBe(value.ToString());
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

    private enum EmptyEnum;

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

    private enum EnumToIgnore
    {
        Foo,
        Bar
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "UnusedTypeParameter")]
    private class GenericType<TFoo>
    {
        public enum EnumInGenericType
        {
            Foo = 42
        }

        public class AnotherOne<TBar>
        {
            public enum EnumInGenericType2
            {
                Foo = 42
            }
        }
    }
}
