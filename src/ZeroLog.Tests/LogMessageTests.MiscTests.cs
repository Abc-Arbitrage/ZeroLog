using System;
using System.Diagnostics;
using NUnit.Framework;
using Shouldly;

namespace ZeroLog.Tests;

partial class LogMessageTests
{
    [TestFixture]
    public class MiscTests : LogMessageTests
    {
        [Test]
        public void should_write_empty_message()
            => LogMessage.Empty.ToString().ShouldBeEmpty();

        [Test]
        public void should_write_constant_message()
            => new LogMessage("foobar").ToString().ShouldBe("foobar");

        [Test]
        public void should_truncate_value_types_after_string_capacity_is_exceeded()
        {
            _logMessage = LogMessage.CreateTestMessage(LogLevel.Info, _bufferLength, 1);

            _logMessage.Append("foo")
                       .Append(10)
                       .Append("bar")
                       .Append(20)
                       .ToString()
                       .ShouldBe(
                           ZeroLogTests.SourceGeneratorsEnabled
                               ? "foo10bar20"
                               : "foo10 [TRUNCATED]"
                       );
        }

        [Test]
        public void should_assign_exception()
        {
            var ex = new InvalidOperationException();
            _logMessage.WithException(ex);

            _logMessage.Exception.ShouldBeSameAs(ex);
        }

        [Test]
        public void should_append_indirect()
        {
            _logMessage.Append($"foo {new LogMessage.AppendOperation<int>(40, static (msg, i) => msg.Append(i + 2))} bar")
                       .ToString()
                       .ShouldBe("foo 42 bar");
        }
    }
}
