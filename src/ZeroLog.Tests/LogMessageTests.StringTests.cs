using System;
using System.Diagnostics.CodeAnalysis;
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
            _logMessage.InternalAppend("foo");

            _logMessage.ToString().ShouldEqual("foo");
        }

        [Test]
        public void should_append_null_string()
        {
            _logMessage.InternalAppend((string?)null);

            _logMessage.ToString().ShouldEqual(LogManager.Config.NullDisplayString);
        }

        [Test]
        public void should_append_strings()
        {
            _logMessage.InternalAppend("foo");
            _logMessage.InternalAppend("bar");

            _logMessage.ToString().ShouldEqual("foobar");
        }

        [Test]
        public void should_truncate_when_string_capacity_is_exceeded()
        {
            _logMessage.InternalAppend("1,");
            _logMessage.InternalAppend("2,");
            _logMessage.InternalAppend("3,");
            _logMessage.InternalAppend("4,");
            _logMessage.InternalAppend("5,");
            _logMessage.InternalAppend("6");

            _logMessage.ToString().ShouldEqual("1,2,3,4, [TRUNCATED]");
        }

        [Test]
        public void should_truncate_null_string_when_output_buffer_is_not_large_enough()
        {
            _logMessage.InternalAppend((string?)null);

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
                    _logMessage.InternalAppend("foo");
            });
        }

        [Test]
        public void should_write_truncated_string()
        {
            _logMessage.InternalAppend("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            string.Create(16, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual("0123 [TRUNCATED]");
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void should_write_truncated_string_when_buffer_is_too_small_to_fit_the_suffix()
        {
            _logMessage.InternalAppend("0123456789");

            string.Create(8, _logMessage, (buffer, message) => message.WriteTo(buffer))
                  .ShouldEqual(" [TRUNCA");
        }
    }
}
