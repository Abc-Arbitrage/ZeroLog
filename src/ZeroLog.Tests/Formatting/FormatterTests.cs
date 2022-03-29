using System;
using NUnit.Framework;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class FormatterTests
{
    private TestFormatter _formatter;

    [SetUp]
    public void SetUp()
    {
        _formatter = new TestFormatter();
    }

    [Test]
    public void should_append()
    {
        _formatter.Write("Foo");
        _formatter.Write("Bar");

        _formatter.GetOutput().SequenceEqual("FooBar").ShouldBeTrue();
    }

    [Test]
    public void should_append_newline()
    {
        _formatter.Write("Foo");
        _formatter.WriteLine();
        _formatter.Write("Bar");

        _formatter.GetOutput().SequenceEqual($"Foo{Environment.NewLine}Bar").ShouldBeTrue();
    }

    [Test]
    public void should_append_newline_2()
    {
        _formatter.WriteLine("Foo");
        _formatter.Write("Bar");

        _formatter.GetOutput().SequenceEqual($"Foo{Environment.NewLine}Bar").ShouldBeTrue();
    }

    [Test]
    public void should_not_overflow()
    {
        var valueA = new string('a', TestFormatter.BufferLength);
        var valueB = new string('b', TestFormatter.BufferLength);

        _formatter.Write(valueA);
        _formatter.Write(valueB);

        _formatter.GetOutput().SequenceEqual(valueA).ShouldBeTrue();
    }

    [Test]
    public void should_append_newline_when_buffer_is_full()
    {
        var value = new string('a', TestFormatter.BufferLength);

        _formatter.Write(value);
        _formatter.WriteLine();

        _formatter.GetOutput().SequenceEqual(value[..^Environment.NewLine.Length] + Environment.NewLine).ShouldBeTrue();
    }

    private class TestFormatter : Formatter
    {
        public static int BufferLength { get; } = new TestFormatter().GetRemainingBuffer().Length;

        protected override void WriteMessage(LoggedMessage message)
        {
        }
    }
}
