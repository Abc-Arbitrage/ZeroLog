using System;
using NUnit.Framework;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests;

unsafe partial class LogMessageTests
{
    [TestFixture]
    public class MiscTests : LogMessageTests
    {
        [Test]
        public void should_write_empty_message()
            => LogMessage.Empty.ToString().ShouldBeEmpty();

        [Test]
        public void should_write_constant_message()
            => new LogMessage("foobar").ToString().ShouldEqual("foobar");

        [Test]
        public void should_truncate_value_types_after_string_capacity_is_exceeded()
        {
            _logMessage = new LogMessage(new BufferSegment(_buffer, _bufferLength), 1);
            _logMessage.Initialize(null, LogLevel.Info);

            _logMessage.Append("foo")
                       .Append(10)
                       .Append("bar")
                       .Append(20)
                       .ToString()
                       .ShouldEqual("foo10 [TRUNCATED]");
        }

        [Test]
        public void should_assign_exception()
        {
            var ex = new InvalidOperationException();
            _logMessage.WithException(ex);

            _logMessage.Exception.ShouldBeTheSameAs(ex);
        }

        [Test]
        public void should_append_indirect()
        {
            _logMessage.Append($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar")
                       .ToString()
                       .ShouldEqual("foo 42 bar");
        }
    }
}
