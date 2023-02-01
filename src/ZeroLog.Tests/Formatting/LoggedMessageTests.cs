using System;
using System.Text;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class LoggedMessageTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 16;

    private LogMessage _logMessage;

    static LoggedMessageTests()
    {
        LogManager.RegisterEnum<DayOfWeek>();
    }

    [SetUp]
    public void SetUp()
    {
        _logMessage = LogMessage.CreateTestMessage(LogLevel.Info, _bufferLength, _stringCapacity);
    }

    [Test]
    public void should_format_message()
    {
        _logMessage.Append("Foo").Append("Bar");
        GetFormatted().ToString().ShouldEqual("FooBar");
    }

    [Test]
    public void should_clone_message()
    {
        _logMessage.Append("Foo")
                   .Append("Bar")
                   .AppendKeyValue("Fizz", "Buzz")
                   .AppendKeyValue("Hello", "World")
                   .WithException(new InvalidOperationException("Oops"));

        var original = GetFormatted();
        var clone = original.Clone();

        clone.ShouldNotBeTheSameAs(original);
        clone.Level.ShouldEqual(original.Level);
        clone.Timestamp.ShouldEqual(original.Timestamp);
        clone.Thread.ShouldEqual(original.Thread);
        clone.Exception.ShouldEqual(original.Exception);
        clone.LoggerName.ShouldEqual(original.LoggerName);
        clone.Message.ToString().ShouldEqual(original.Message.ToString());

        clone.KeyValues.ShouldNotBeTheSameAs(original.KeyValues);
        clone.KeyValues.Count.ShouldEqual(original.KeyValues.Count);

        for (var i = 0; i < original.KeyValues.Count; ++i)
        {
            clone.KeyValues[i].Key.ShouldEqual(original.KeyValues[i].Key);
            clone.KeyValues[i].Value.ToString().ShouldEqual(original.KeyValues[i].Value.ToString());
        }
    }

    [Test]
    public void should_clone_message_twice()
    {
        _logMessage.Append("Foo")
                   .Append("Bar")
                   .AppendKeyValue("Fizz", "Buzz")
                   .AppendKeyValue("Hello", "World")
                   .WithException(new InvalidOperationException("Oops"));

        var original = GetFormatted();
        var clone = original.Clone().Clone();

        clone.ShouldNotBeTheSameAs(original);
        clone.Level.ShouldEqual(original.Level);
        clone.Timestamp.ShouldEqual(original.Timestamp);
        clone.Thread.ShouldEqual(original.Thread);
        clone.Exception.ShouldEqual(original.Exception);
        clone.LoggerName.ShouldEqual(original.LoggerName);
        clone.Message.ToString().ShouldEqual(original.Message.ToString());

        clone.KeyValues.ShouldNotBeTheSameAs(original.KeyValues);
        clone.KeyValues.Count.ShouldEqual(original.KeyValues.Count);

        for (var i = 0; i < original.KeyValues.Count; ++i)
        {
            clone.KeyValues[i].Key.ShouldEqual(original.KeyValues[i].Key);
            clone.KeyValues[i].Value.ToString().ShouldEqual(original.KeyValues[i].Value.ToString());
        }
    }

    [Test]
    public void should_keep_cloned_data_after_reset()
    {
        _logMessage.Append("Foo")
                   .Append("Bar")
                   .AppendKeyValue("Fizz", "Buzz")
                   .AppendKeyValue("Hello", DayOfWeek.Friday)
                   .WithException(new InvalidOperationException("Oops"));

        var original = GetFormatted();
        var clone = original.Clone();

        var dataBeforeReset = (
            clone.Level,
            clone.Timestamp,
            clone.Thread,
            clone.Exception,
            clone.LoggerName,
            clone.Message.ToString(),
            CaptureKeyValueList(clone.KeyValues)
        );

        _logMessage.Initialize(null, LogLevel.Warn);
        original.SetMessage(LogMessage.CreateTestMessage(LogLevel.Warn, 0, 0));

        var dataAfterReset = (
            clone.Level,
            clone.Timestamp,
            clone.Thread,
            clone.Exception,
            clone.LoggerName,
            clone.Message.ToString(),
            CaptureKeyValueList(clone.KeyValues)
        );

        dataAfterReset.ShouldEqual(dataBeforeReset);

        static string CaptureKeyValueList(KeyValueList list)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
                sb.Append($"[{item.Key}]={item.Value}({item.ArgumentType}={item.ValueType?.Name})");
            return sb.ToString();
        }
    }

    [Test]
    public void should_read_value_byte()
    {
        _logMessage.AppendKeyValue("Foo", (byte)42);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(byte));
        result.TryGetValue<byte>(out var value).ShouldBeTrue();
        value.ShouldEqual((byte)42);
    }

    [Test]
    public void should_read_value_sbyte()
    {
        _logMessage.AppendKeyValue("Foo", (sbyte)-42);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(sbyte));
        result.TryGetValue<sbyte>(out var value).ShouldBeTrue();
        value.ShouldEqual((sbyte)-42);
    }

    [Test]
    public void should_read_value_int16()
    {
        _logMessage.AppendKeyValue("Foo", (short)-42);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(short));
        result.TryGetValue<short>(out var value).ShouldBeTrue();
        value.ShouldEqual((short)-42);
    }

    [Test]
    public void should_read_value_uint16()
    {
        _logMessage.AppendKeyValue("Foo", (ushort)42);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(ushort));
        result.TryGetValue<ushort>(out var value).ShouldBeTrue();
        value.ShouldEqual((ushort)42u);
    }

    [Test]
    public void should_read_value_int32()
    {
        _logMessage.AppendKeyValue("Foo", -42);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(int));
        result.TryGetValue<int>(out var value).ShouldBeTrue();
        value.ShouldEqual(-42);

        result.TryGetValue<uint>(out _).ShouldBeFalse();
    }

    [Test]
    public void should_read_value_uint32()
    {
        _logMessage.AppendKeyValue("Foo", 42u);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(uint));
        result.TryGetValue<uint>(out var value).ShouldBeTrue();
        value.ShouldEqual(42u);

        result.TryGetValue<int>(out _).ShouldBeFalse();
    }

    [Test]
    public void should_read_value_int64()
    {
        _logMessage.AppendKeyValue("Foo", -42L);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(long));
        result.TryGetValue<long>(out var value).ShouldBeTrue();
        value.ShouldEqual(-42L);
    }

    [Test]
    public void should_read_value_uint64()
    {
        _logMessage.AppendKeyValue("Foo", 42ul);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(ulong));
        result.TryGetValue<ulong>(out var value).ShouldBeTrue();
        value.ShouldEqual(42ul);
    }

    [Test]
    public void should_read_value_guid()
    {
        var guid = Guid.NewGuid();
        _logMessage.AppendKeyValue("Foo", guid);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(Guid));
        result.TryGetValue<Guid>(out var value).ShouldBeTrue();
        value.ShouldEqual(guid);
    }

    [Test]
    public void should_read_value_enum()
    {
        _logMessage.AppendKeyValue("Foo", DayOfWeek.Friday);

        var result = GetFirstValue();
        result.ValueType.ShouldEqual(typeof(DayOfWeek));
        result.TryGetValue<DayOfWeek>(out var value).ShouldBeTrue();
        value.ShouldEqual(DayOfWeek.Friday);
    }

    [Test]
    public void should_return_string_type_for_string()
    {
        _logMessage.AppendKeyValue("Foo", "Bar");
        GetFirstValue().ValueType.ShouldEqual(typeof(string));
    }

    [Test]
    public void should_return_string_type_for_span_of_byte()
    {
        _logMessage.AppendKeyValue("Foo", new[] { (byte)'A' });
        GetFirstValue().ValueType.ShouldEqual(typeof(string));
    }

    [Test]
    public void should_return_string_type_for_span_of_char()
    {
        _logMessage.AppendKeyValue("Foo", "Bar".AsSpan());
        GetFirstValue().ValueType.ShouldEqual(typeof(string));
    }

    private LoggedMessage GetFormatted()
    {
        var message = new LoggedMessage(_bufferLength, ZeroLogConfiguration.Default);
        message.SetMessage(_logMessage);
        return message;
    }

    private LoggedKeyValue GetFirstValue()
    {
        foreach (var value in GetFormatted().KeyValues)
            return value;

        throw new InvalidOperationException("No values logged");
    }
}
