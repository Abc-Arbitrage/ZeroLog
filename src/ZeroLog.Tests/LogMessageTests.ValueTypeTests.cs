using System;
using System.Globalization;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

unsafe partial class LogMessageTests
{
    public class ValueTypeTests<T> : LogMessageTests
        where T : unmanaged
    {
        protected void ShouldTruncateValue(Action action, bool formatted)
        {
            var requiredBufferSize = sizeof(ArgumentType) + sizeof(T) + (formatted ? sizeof(byte) : 0);

            // Ensure the message fits in the buffer as a sanity check
            _logMessage = new LogMessage(new BufferSegment(_buffer, requiredBufferSize), _stringCapacity);
            _logMessage.Initialize(null, Level.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeFalse();

            if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) && typeof(T) != typeof(char))
            {
                // Truncate because the output buffer is too small
                Span<char> smallBuffer = stackalloc char[2];
                _logMessage.WriteTo(smallBuffer).ShouldEqual(smallBuffer.Length);
                smallBuffer.SequenceEqual(LogManager.Config.TruncatedMessageSuffix.AsSpan(0, smallBuffer.Length)).ShouldBeTrue();
            }

            // Edge case: empty output buffer
            _logMessage.WriteTo(Span<char>.Empty).ShouldEqual(0);

            // Truncate because the log message buffer is too small
            _logMessage = new LogMessage(new BufferSegment(_buffer, requiredBufferSize - 1), _stringCapacity);
            _logMessage.Initialize(null, Level.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeTrue();
            _logMessage.ToString().ShouldEqual(LogManager.Config.TruncatedMessageSuffix);

            // Edge case: empty log message buffer
            _logMessage = new LogMessage(new BufferSegment(_buffer, 0), _stringCapacity);
            _logMessage.Initialize(null, Level.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeTrue();
        }

        // The following methods help reduce the number of casts in the code,
        // which ensures the correct overload is called after the code is copied from one type to another.

        protected static T? AsNullable(T value)
            => value;

        protected static T? GetNullValue()
            => null;
    }

    [TestFixture]
    public class BoolTests : ValueTypeTests<bool>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_value_through_interpolation([Values] bool value)
            => _logMessage.Append($"{value}").ToString().ShouldEqual(value.ToString());

        [Test]
        public void should_append_nullable_value_through_interpolation([Values] bool value)
            => _logMessage.Append($"{AsNullable(value)}").ToString().ShouldEqual(value.ToString());

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));
    }

    [TestFixture]
    public class ByteTests : ValueTypeTests<byte>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_formatted()
            => ShouldNotAllocate(() => _logMessage.Append(_value, "X"));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class SByteTests : ValueTypeTests<sbyte>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_formatted()
            => ShouldNotAllocate(() => _logMessage.Append(_value, "X"));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class CharTests : ValueTypeTests<char>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));
    }

    [TestFixture]
    public class ShortTests : ValueTypeTests<short>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class UShortTests : ValueTypeTests<ushort>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class Int32Tests : ValueTypeTests<int>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class UInt32Tests : ValueTypeTests<uint>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class Int64Tests : ValueTypeTests<long>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class UInt64Tests : ValueTypeTests<ulong>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class IntPtrTests : ValueTypeTests<nint>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class UIntPtrTests : ValueTypeTests<nuint>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "X").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "X").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):X}").ToString().ShouldEqual(_value.ToString("X", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():X}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "X"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("X")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:X}"));
    }

    [TestFixture]
    public class SingleTests : ValueTypeTests<float>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "F2").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():F2}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "F2"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(42.5f));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:F2}"));
    }

    [TestFixture]
    public class DoubleTests : ValueTypeTests<double>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "F2").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():F2}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "F2"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:F2}"));
    }

    [TestFixture]
    public class DecimalTests : ValueTypeTests<decimal>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value()
            => _logMessage.Append(_value, "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "F2").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(AsNullable(_value), "F2").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):F2}").ToString().ShouldEqual(_value.ToString("F2", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():F2}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "F2"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("P")]
        [TestCase("F2")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:F2}"));
    }

    [TestFixture]
    public class GuidTests : ValueTypeTests<Guid>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "D").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "D").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:D}").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):D}").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():D}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "D"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("N")]
        [TestCase("D")]
        [TestCase("B")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:B}"));
    }

    [TestFixture]
    public class DateTimeTests : ValueTypeTests<DateTime>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("d")]
        [TestCase("D")]
        [TestCase("yyyy-MM-dd")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "D").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "D").ToString().ShouldEqual(_value.ToString("D", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:yyyy}").ToString().ShouldEqual(_value.ToString("yyyy", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):yyyy}").ToString().ShouldEqual(_value.ToString("yyyy", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():yyyy}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "D"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("d")]
        [TestCase("D")]
        [TestCase("yyyy-MM-dd")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:yyyy-MM-dd}"));
    }

    [TestFixture]
    public class TimeSpanTests : ValueTypeTests<TimeSpan>
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
            => _logMessage.Append(GetNullValue()).ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        [TestCase("c")]
        [TestCase("G")]
        public void should_append_formatted_value(string format)
            => _logMessage.Append(_value, format).ToString().ShouldEqual(_value.ToString(format, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null()
            => _logMessage.Append(GetNullValue(), "G").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_nullable_formatted_value()
            => _logMessage.Append(_value, "c").ToString().ShouldEqual(_value.ToString("c", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{_value}").ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value)}").ToString().ShouldEqual(_value.ToString(null, CultureInfo.InvariantCulture));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue()}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_append_formatted_value_through_interpolation()
            => _logMessage.Append($"{_value:c}").ToString().ShouldEqual(_value.ToString("c", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_nullable_formatted_value_through_interpolation()
            => _logMessage.Append($"{AsNullable(_value):c}").ToString().ShouldEqual(_value.ToString("c", CultureInfo.InvariantCulture));

        [Test]
        public void should_append_formatted_null_value_through_interpolation()
            => _logMessage.Append($"{GetNullValue():c}").ToString().ShouldEqual(LogManager.Config.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value), false);

        [Test]
        public void should_truncate_formatted_value()
            => ShouldTruncateValue(() => _logMessage.Append(_value, "c"), true);

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.Append(_value));

        [Test]
        [TestCase("c")]
        [TestCase("G")]
        public void should_not_allocate_formatted(string format)
            => ShouldNotAllocate(() => _logMessage.Append(_value, format));

        [Test]
        public void should_not_allocate_interpolation()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value}"));

        [Test]
        public void should_not_allocate_interpolation_formatted()
            => ShouldNotAllocate(() => _logMessage.Append($"{_value:c}"));
    }
}
