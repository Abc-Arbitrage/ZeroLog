using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

#nullable enable

unsafe partial class LogMessageTests
{
    [TestFixture]
    public class StringTests : LogMessageTests
    {
        private static string NoInline(string value)
            => value;

        [Test]
        public void should_append_string()
        {
            _logMessage.Append("foo");

            _logMessage.ToString().ShouldEqual("foo");
        }

        [Test]
        public void should_append_null_string()
        {
            _logMessage.Append((string?)null);

            _logMessage.ToString().ShouldEqual(ZeroLogConfiguration.Default.NullDisplayString);
        }

        [Test]
        public void should_append_strings()
        {
            _logMessage.Append("foo")
                       .Append("bar");

            _logMessage.ToString().ShouldEqual("foobar");
        }

        [Test]
        public void should_append_through_string_interpolation()
        {
            _logMessage.Append($"foo {NoInline("bar")} baz");

            _logMessage.ToString().ShouldEqual("foo bar baz");
        }

        [Test]
        public void should_truncate_when_string_capacity_is_exceeded()
        {
            _logMessage.Append("1,")
                       .Append("2,")
                       .Append("3,")
                       .Append("4,")
                       .Append("5,")
                       .Append("6");

            _logMessage.ToString().ShouldEqual("1,2,3,4, [TRUNCATED]");
        }

        [Test]
        public void should_truncate_null_string_when_output_buffer_is_not_large_enough()
        {
            _logMessage.Append((string?)null);

            Span<char> outputBuffer = stackalloc char[2];
            _logMessage.WriteTo(outputBuffer, ZeroLogConfiguration.Default).ShouldEqual(outputBuffer.Length);
            outputBuffer.SequenceEqual(ZeroLogConfiguration.Default.NullDisplayString.AsSpan(0, outputBuffer.Length));
        }

        [Test]
        public void should_not_allocate()
        {
            ShouldNotAllocate(() =>
            {
                for (var i = 0; i < 2 * _stringCapacity; ++i)
                    _logMessage.Append("foo");
            });
        }

        [Test]
        public void should_not_allocate_interpolation()
        {
            ShouldNotAllocate(() =>
            {
                for (var i = 0; i < 2 * _stringCapacity; ++i)
                    _logMessage.Append($"foo {NoInline("bar")}");
            });
        }

        [Test]
        public void should_write_truncated_string()
        {
            _logMessage.Append("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.Append("0123456789");

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual(" [TRUNCA");
        }
    }

    [TestFixture]
    public class CharAsciiStringTests : LogMessageTests
    {
        [Test]
        public void should_append_string()
        {
            _logMessage.AppendAsciiString("foo").ShouldEqual(_logMessage);

            _logMessage.ToString().ShouldEqual("foo");
        }

        [Test]
        public void should_append_empty_string()
        {
            _logMessage.AppendAsciiString(ReadOnlySpan<char>.Empty);

            _logMessage.ToString().ShouldEqual(string.Empty);
        }

        [Test]
        public void should_append_strings()
        {
            _logMessage.AppendAsciiString("foo");
            _logMessage.AppendAsciiString("bar");

            _logMessage.ToString().ShouldEqual("foobar");
        }

        [Test]
        [TestCase("0123", "0123")]
        [TestCase("01234", "01234")]
        [TestCase("012346", "01234 [TRUNCATED]")]
        public void should_truncate_when_buffer_size_is_exceeded(string value, string expected)
        {
            _logMessage = new LogMessage(new BufferSegment(_buffer, sizeof(ArgumentType) + sizeof(int) + 5, null), _stringCapacity);
            _logMessage.Initialize(null, LogLevel.Info);

            _logMessage.AppendAsciiString(value);

            _logMessage.ToString().ShouldEqual(expected);
        }

        [Test]
        public void should_not_allocate()
        {
            ShouldNotAllocate(() =>
            {
                for (var i = 0; i < 2 * _stringCapacity; ++i)
                    _logMessage.AppendAsciiString("foo");
            });
        }

        [Test]
        public void should_write_truncated_string()
        {
            _logMessage.AppendAsciiString("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.AppendAsciiString("0123456789");

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual(" [TRUNCA");
        }
    }

    [TestFixture]
    public class ByteAsciiStringTests : LogMessageTests
    {
        [Test]
        public void should_append_string()
        {
            _logMessage.AppendAsciiString(GetBytes("foo")).ShouldEqual(_logMessage);

            _logMessage.ToString().ShouldEqual("foo");
        }

        [Test]
        public void should_append_empty_string()
        {
            _logMessage.AppendAsciiString(ReadOnlySpan<byte>.Empty);

            _logMessage.ToString().ShouldEqual(string.Empty);
        }

        [Test]
        public void should_append_strings()
        {
            _logMessage.AppendAsciiString(GetBytes("foo"));
            _logMessage.AppendAsciiString(GetBytes("bar"));

            _logMessage.ToString().ShouldEqual("foobar");
        }

        [Test]
        [TestCase("0123", "0123")]
        [TestCase("01234", "01234")]
        [TestCase("012346", "01234 [TRUNCATED]")]
        public void should_truncate_when_buffer_size_is_exceeded(string value, string expected)
        {
            _logMessage = new LogMessage(new BufferSegment(_buffer, sizeof(ArgumentType) + sizeof(int) + 5, null), _stringCapacity);
            _logMessage.Initialize(null, LogLevel.Info);

            _logMessage.AppendAsciiString(GetBytes(value));

            _logMessage.ToString().ShouldEqual(expected);
        }

        [Test]
        public void should_not_allocate()
        {
            var value = GetBytes("foo");

            ShouldNotAllocate(() =>
            {
                for (var i = 0; i < 2 * _stringCapacity; ++i)
                    _logMessage.AppendAsciiString(value);
            });
        }

        [Test]
        public void should_write_truncated_string()
        {
            _logMessage.AppendAsciiString(GetBytes("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"));

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.AppendAsciiString(GetBytes("0123456789"));

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer, ZeroLogConfiguration.Default))
                  .ShouldEqual(" [TRUNCA");
        }

        private static byte[] GetBytes(string value)
            => Encoding.ASCII.GetBytes(value);
    }
}
