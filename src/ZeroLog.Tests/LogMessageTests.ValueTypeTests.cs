using System;
using System.Globalization;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

partial class LogMessageTests
{
    [TestFixture]
    public class BoolTests : LogMessageTests
    {
        [Test]
        public void should_append_value([Values] bool value)
            => _logMessage.Append(value).ToString().ShouldEqual(value.ToString());

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((bool?)true).ToString().ShouldEqual(bool.TrueString);

        [Test]
        public void should_append_null()
            => _logMessage.Append((bool?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(true));
    }

    [TestFixture]
    public class ByteTests : LogMessageTests
    {
        private const byte _value = 42;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((byte?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((byte?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class CharTests : LogMessageTests
    {
        private const char _value = 'x';

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((char?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((char?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class ShortTests : LogMessageTests
    {
        private const short _value = short.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((short?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((short?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class Int32Tests : LogMessageTests
    {
        private const int _value = int.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((int?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((int?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class Int64Tests : LogMessageTests
    {
        private const long _value = long.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((long?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((long?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class SingleTests : LogMessageTests
    {
        private const float _value = float.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((float?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((float?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(42.5f));
    }

    [TestFixture]
    public class DoubleTests : LogMessageTests
    {
        private const double _value = double.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((double?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((double?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class DecimalTests : LogMessageTests
    {
        private const decimal _value = decimal.MaxValue;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((decimal?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((decimal?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class GuidTests : LogMessageTests
    {
        private static readonly Guid _value = Guid.NewGuid();

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((Guid?)_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((Guid?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class DateTimeTests : LogMessageTests
    {
        private static readonly DateTime _value = DateTime.UtcNow;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((DateTime?)_value).ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((DateTime?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }

    [TestFixture]
    public class TimeSpanTests : LogMessageTests
    {
        private static readonly TimeSpan _value = DateTime.UtcNow.TimeOfDay;

        [Test]
        public void should_append_value()
            => _logMessage.Append(_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.Append((TimeSpan?)_value).ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null()
            => _logMessage.Append((TimeSpan?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));
    }
}
