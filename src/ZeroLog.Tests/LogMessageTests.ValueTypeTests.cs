using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

partial class LogMessageTests
{
    public class ValueTypeTests : LogMessageTests
    {
        // The following methods help reduce the number of casts in the code,
        // which ensures the correct overload is called after the code is copied from one type to another.

        protected static T? AsNullable<T>(T value)
            where T : struct
            => value;

        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        protected static T? GetNullValue<T>(T value)
            where T : struct
            => null;
    }

    [TestFixture]
    public class BoolTests : ValueTypeTests
    {
        private const bool _value = true;

        [Test]
        public void should_append_value([Values] bool value)
            => _logMessage.Append(value).ToString().ShouldEqual(value.ToString());

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString());

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(true));
    }

    [TestFixture]
    public class ByteTests : ValueTypeTests
    {
        private const byte _value = 42;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_formatted()
            => ShouldNotAllocate(() => _logMessage.Append(_value, "X"));
    }

    [TestFixture]
    public class SByteTests : ValueTypeTests
    {
        private const sbyte _value = -42;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_formatted()
            => ShouldNotAllocate(() => _logMessage.Append(_value, "X"));
    }

    [TestFixture]
    public class CharTests : ValueTypeTests
    {
        private const char _value = 'x';

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class ShortTests : ValueTypeTests
    {
        private const short _value = short.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class UShortTests : ValueTypeTests
    {
        private const ushort _value = ushort.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class Int32Tests : ValueTypeTests
    {
        private const int _value = int.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class UInt32Tests : ValueTypeTests
    {
        private const uint _value = uint.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class Int64Tests : ValueTypeTests
    {
        private const long _value = long.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class UInt64Tests : ValueTypeTests
    {
        private const ulong _value = ulong.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class IntPtrTests : ValueTypeTests
    {
        private static readonly nint _value = nint.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class UIntPtrTests : ValueTypeTests
    {
        private static readonly nuint _value = nuint.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class SingleTests : ValueTypeTests
    {
        private const float _value = float.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(42.5f));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class DoubleTests : ValueTypeTests
    {
        private const double _value = double.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class DecimalTests : ValueTypeTests
    {
        private const decimal _value = decimal.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class GuidTests : ValueTypeTests
    {
        private static readonly Guid _value = Guid.NewGuid();

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "D").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class DateTimeTests : ValueTypeTests
    {
        private static readonly DateTime _value = DateTime.UtcNow;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("d")]
        [TestCase("D")]
        [TestCase("yyyy-MM-dd")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "D").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("d")]
        [TestCase("D")]
        [TestCase("yyyy-MM-dd")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }

    [TestFixture]
    public class TimeSpanTests : ValueTypeTests
    {
        private static readonly TimeSpan _value = DateTime.UtcNow.TimeOfDay;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append(AsNullable(_value)).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append(GetNullValue(_value)).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("c")]
        [TestCase("G")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "c").ToString().ShouldEqual(_value.ToString("c", CultureInfo.InvariantCulture));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("c")]
        [TestCase("G")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));
    }
}
