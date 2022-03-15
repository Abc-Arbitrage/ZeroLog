using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

unsafe partial class LogMessageTests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public class EnumTests : LogMessageTests
    {
        static EnumTests()
        {
            LogManager.RegisterEnum<DayOfWeek>();
        }

        [Test]
        public void should_append_value()
            => _logMessage.AppendEnum(DayOfWeek.Friday).ToString().ShouldEqual(nameof(DayOfWeek.Friday));

        [Test]
        public void should_append_nullable_value()
            => _logMessage.AppendEnum((DayOfWeek?)DayOfWeek.Friday).ToString().ShouldEqual(nameof(DayOfWeek.Friday));

        [Test]
        public void should_append_null()
            => _logMessage.AppendEnum((DayOfWeek?)null).ToString().ShouldEqual(ZeroLogConfiguration.Default.NullDisplayString);

        [Test]
        public void should_append_numeric_value()
            => _logMessage.AppendEnum((DayOfWeek)42).ToString().ShouldEqual("42");

        [Test]
        public void should_append_negative_numeric_value()
            => _logMessage.AppendEnum((DayOfWeek)(-42)).ToString().ShouldEqual("-42");

        [Test]
        public void should_append_nullable_numeric_value()
            => _logMessage.AppendEnum((DayOfWeek?)42).ToString().ShouldEqual("42");

        [Test]
        public void should_append_value_through_interpolation()
            => _logMessage.Append($"{DayOfWeek.Friday}").ToString().ShouldEqual(nameof(DayOfWeek.Friday));

        [Test]
        public void should_append_nullable_value_through_interpolation()
            => _logMessage.Append($"{(DayOfWeek?)DayOfWeek.Friday}").ToString().ShouldEqual(nameof(DayOfWeek.Friday));

        [Test]
        public void should_append_null_value_through_interpolation()
            => _logMessage.Append($"{(DayOfWeek?)null}").ToString().ShouldEqual(ZeroLogConfiguration.Default.NullDisplayString);

        [Test]
        public void should_truncate_value()
            => ShouldTruncateValue(() => _logMessage.AppendEnum(DayOfWeek.Friday));

        [Test]
        public void should_truncate_numeric_value()
            => ShouldTruncateValue(() => _logMessage.AppendEnum((DayOfWeek)420));

        [Test]
        public void should_truncate_negative_numeric_value()
            => ShouldTruncateValue(() => _logMessage.AppendEnum((DayOfWeek)(-420)));

        [Test]
        public void should_not_allocate()
            => ShouldNotAllocate(() => _logMessage.AppendEnum(DayOfWeek.Friday));

        private void ShouldTruncateValue(Action action)
        {
            var requiredBufferSize = sizeof(ArgumentType) + sizeof(EnumArg);

            // Ensure the message fits in the buffer as a sanity check
            _logMessage = new LogMessage(new BufferSegment(_buffer, requiredBufferSize, null), _stringCapacity);
            _logMessage.Initialize(null, LogLevel.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeFalse();

            // Truncate because the output buffer is too small
            Span<char> smallBuffer = stackalloc char[2];
            _logMessage.WriteTo(smallBuffer, ZeroLogConfiguration.Default).ShouldEqual(smallBuffer.Length);
            smallBuffer.SequenceEqual(ZeroLogConfiguration.Default.TruncatedMessageSuffix.AsSpan(0, smallBuffer.Length)).ShouldBeTrue();

            // Edge case: empty output buffer
            _logMessage.WriteTo(Span<char>.Empty, ZeroLogConfiguration.Default).ShouldEqual(0);

            // Truncate because the log message buffer is too small
            _logMessage = new LogMessage(new BufferSegment(_buffer, requiredBufferSize - 1, null), _stringCapacity);
            _logMessage.Initialize(null, LogLevel.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeTrue();
            _logMessage.ToString().ShouldEqual(ZeroLogConfiguration.Default.TruncatedMessageSuffix);

            // Edge case: empty log message buffer
            _logMessage = new LogMessage(new BufferSegment(_buffer, 0, null), _stringCapacity);
            _logMessage.Initialize(null, LogLevel.Info);

            action.Invoke();

            _logMessage.IsTruncated.ShouldBeTrue();
        }
    }
}
