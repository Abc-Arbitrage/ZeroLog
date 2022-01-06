using System;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

partial class LogMessageTests
{
    [TestFixture]
    public class UnmanagedTests : LogMessageTests
    {
        static UnmanagedTests()
        {
            LogManager.RegisterUnmanaged<Int64FormattableWrapper>();

            LogManager.RegisterUnmanaged((ref ForwardFormatToOutputStruct _, Span<char> destination, out int written, ReadOnlySpan<char> format) =>
            {
                written = format.Length;
                return format.TryCopyTo(destination);
            });
        }

        [Test]
        public void should_append_formattable_value()
        {
            var value = new Int64FormattableWrapper { Value = 42 };
            _logMessage.AppendUnmanaged(value).ToString().ShouldEqual("42");
        }

        [Test]
        public void should_append_formattable_value_ref()
        {
            var value = new Int64FormattableWrapper { Value = 42 };
            _logMessage.AppendUnmanaged(ref value).ToString().ShouldEqual("42");
        }

        [Test]
        public void should_append_formattable_value_nullable()
        {
            Int64FormattableWrapper? value = new Int64FormattableWrapper { Value = 42 };
            _logMessage.AppendUnmanaged(value).ToString().ShouldEqual("42");
        }

        [Test]
        public void should_append_formattable_value_nullable_ref()
        {
            Int64FormattableWrapper? value = new Int64FormattableWrapper { Value = 42 };
            _logMessage.AppendUnmanaged(ref value).ToString().ShouldEqual("42");
        }

        [Test]
        public void should_append_null_value()
        {
            _logMessage.AppendUnmanaged((Int64FormattableWrapper?)null).ToString().ShouldEqual(LogManager.Config.NullDisplayString);
        }

        [Test]
        public void should_append_null_value_ref()
        {
            Int64FormattableWrapper? value = null;
            _logMessage.AppendUnmanaged(ref value).ToString().ShouldEqual(LogManager.Config.NullDisplayString);
        }

        [Test]
        public void should_forward_format()
        {
            _logMessage.AppendUnmanaged(new ForwardFormatToOutputStruct()).ToString().ShouldEqual("");
            _logMessage.AppendUnmanaged(new ForwardFormatToOutputStruct(), "foo").ToString().ShouldEqual("foo");
        }

        public struct Int64FormattableWrapper : ISpanFormattable
        {
            public long Value;

            public string ToString(string format, IFormatProvider formatProvider)
                => throw new InvalidOperationException();

            public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
                => Value.TryFormat(destination, out charsWritten, format, provider);
        }

        public struct ForwardFormatToOutputStruct
        {
        }
    }
}
