using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

#nullable enable

unsafe partial class LogMessageTests
{
    [TestFixture]
    public class StringTests : LogMessageTests
    {
        [Test]
        public void should_append_string()
        {
            _logMessage.InternalAppendString("foo");

            _logMessage.ToString().ShouldEqual("foo");
        }

        [Test]
        public void should_append_null_string()
        {
            _logMessage.InternalAppendString(null);

            _logMessage.ToString().ShouldEqual(LogManager.Config.NullDisplayString);
        }

        [Test]
        public void should_append_strings()
        {
            _logMessage.InternalAppendString("foo");
            _logMessage.InternalAppendString("bar");

            _logMessage.ToString().ShouldEqual("foobar");
        }

        [Test]
        public void should_truncate_when_string_capacity_is_exceeded()
        {
            _logMessage.InternalAppendString("1,");
            _logMessage.InternalAppendString("2,");
            _logMessage.InternalAppendString("3,");
            _logMessage.InternalAppendString("4,");
            _logMessage.InternalAppendString("5,");
            _logMessage.InternalAppendString("6");

            _logMessage.ToString().ShouldEqual("1,2,3,4, [TRUNCATED]");
        }

        [Test]
        public void should_truncate_null_string_when_output_buffer_is_not_large_enough()
        {
            _logMessage.InternalAppendString(null);

            Span<char> outputBuffer = stackalloc char[2];
            _logMessage.WriteTo(outputBuffer).ShouldEqual(outputBuffer.Length);
            outputBuffer.SequenceEqual(LogManager.Config.NullDisplayString.AsSpan(0, outputBuffer.Length));
        }

        [Test]
        public void should_not_allocate()
        {
            ShouldNotAllocate(() =>
            {
                for (var i = 0; i < 2 * _stringCapacity; ++i)
                    _logMessage.InternalAppendString("foo");
            });
        }

        [Test]
        public void should_write_truncated_string()
        {
            _logMessage.InternalAppendString("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.InternalAppendString("0123456789");

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer))
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
            _logMessage = new LogMessage(new BufferSegment(_buffer, sizeof(ArgumentType) + sizeof(int) + 5), _stringCapacity);
            _logMessage.Initialize(null, Level.Info);

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

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.AppendAsciiString("0123456789");

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer))
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
            _logMessage = new LogMessage(new BufferSegment(_buffer, sizeof(ArgumentType) + sizeof(int) + 5), _stringCapacity);
            _logMessage.Initialize(null, Level.Info);

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

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.AppendAsciiString(GetBytes("0123456789"));

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual(" [TRUNCA");
        }

        private static byte[] GetBytes(string value)
            => Encoding.ASCII.GetBytes(value);
    }
}
