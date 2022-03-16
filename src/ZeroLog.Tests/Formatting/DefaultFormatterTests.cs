using System;
using System.Text;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class DefaultFormatterTests
{
    private const int _bufferLength = 1024;
    private const int _stringCapacity = 16;

    private LogMessage _logMessage;

    static DefaultFormatterTests()
    {
        LogManager.RegisterEnum<DayOfWeek>();
    }

    [SetUp]
    public void SetUp()
    {
        _logMessage = LogMessage.CreateTestMessage(LogLevel.Info, _bufferLength, _stringCapacity);
    }

    [Test]
    public void should_format_with_default_pattern()
    {
        _logMessage.Initialize(new Log("TestLog"), LogLevel.Info);
        _logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);

        _logMessage.Append("Foo").Append(42);

        GetFormattedFull().ShouldEqual($"03:04:05.0060000 - INFO - TestLog || Foo42{Environment.NewLine}");
    }

    [Test]
    public void should_format_json()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", 42)
                   .AppendKeyValue("World", (int?)10)
                   .AppendKeyValue("String", "foo\r\n")
                   .AppendKeyValue("NullString", (string)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""String"": ""foo\r\n"", ""NullString"": null }");
    }

    [Test]
    public void should_format_json_string()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", "...\u0001...")
                   .AppendKeyValue("Escapes", "\"\\\b\t\n\f\r")
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""...\u0001..."", ""Escapes"": ""\""\\\b\t\n\f\r"" }");
    }

    [Test]
    public void should_format_json_ascii_string_char()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValueAscii("Hello", "World")
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""World"" }");
    }

    [Test]
    public void should_format_json_ascii_string_byte()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValueAscii("Hello", Encoding.ASCII.GetBytes("World"))
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""World"" }");
    }

    [Test]
    public void should_format_json_bool()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", true)
                   .AppendKeyValue("World", false)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": true, ""World"": false }");
    }

    [Test]
    public void should_format_json_byte()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (byte)42)
                   .AppendKeyValue("World", (byte?)10)
                   .AppendKeyValue("Null", (byte?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_sbyte()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (sbyte)42)
                   .AppendKeyValue("World", (sbyte?)10)
                   .AppendKeyValue("Null", (sbyte?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_char()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", 'x')
                   .AppendKeyValue("World", (char?)'y')
                   .AppendKeyValue("Null", (char?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""x"", ""World"": ""y"", ""Null"": null }");
    }

    [Test]
    public void should_format_json_short()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (short)42)
                   .AppendKeyValue("World", (short?)10)
                   .AppendKeyValue("Null", (short?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_ushort()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (ushort)42)
                   .AppendKeyValue("World", (ushort?)10)
                   .AppendKeyValue("Null", (ushort?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_int()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (int)42)
                   .AppendKeyValue("World", (int?)10)
                   .AppendKeyValue("Null", (int?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_uint()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (uint)42)
                   .AppendKeyValue("World", (uint?)10)
                   .AppendKeyValue("Null", (uint?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_long()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (long)42)
                   .AppendKeyValue("World", (long?)10)
                   .AppendKeyValue("Null", (long?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_ulong()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (ulong)42)
                   .AppendKeyValue("World", (ulong?)10)
                   .AppendKeyValue("Null", (ulong?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_nint()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (nint)42)
                   .AppendKeyValue("World", (nint?)10)
                   .AppendKeyValue("Null", (nint?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_nuint()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", (nuint)42)
                   .AppendKeyValue("World", (nuint?)10)
                   .AppendKeyValue("Null", (nuint?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42, ""World"": 10, ""Null"": null }");
    }

    [Test]
    public void should_format_json_single()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", 42.1f)
                   .AppendKeyValue("World", (float?)42.2f)
                   .AppendKeyValue("Null", (float?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42.1, ""World"": 42.2, ""Null"": null }");
    }

    [Test]
    public void should_format_json_double()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", 42.1)
                   .AppendKeyValue("World", (double?)42.2)
                   .AppendKeyValue("Null", (double?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42.1, ""World"": 42.2, ""Null"": null }");
    }

    [Test]
    public void should_format_json_decimal()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", 42.1m)
                   .AppendKeyValue("World", (decimal?)42.2m)
                   .AppendKeyValue("Null", (decimal?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": 42.1, ""World"": 42.2, ""Null"": null }");
    }

    [Test]
    public void should_format_json_guid()
    {
        var value = Guid.NewGuid();

        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", value)
                   .AppendKeyValue("World", (Guid?)value)
                   .AppendKeyValue("Null", (Guid?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": """ + value + @""", ""World"": """ + value + @""", ""Null"": null }");
    }

    [Test]
    public void should_format_json_date()
    {
        var value = new DateTime(2020, 01, 02, 03, 04, 05);

        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", value)
                   .AppendKeyValue("World", (DateTime?)value)
                   .AppendKeyValue("Null", (DateTime?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""2020-01-02 03:04:05"", ""World"": ""2020-01-02 03:04:05"", ""Null"": null }");
    }

    [Test]
    public void should_format_json_time()
    {
        var value = new TimeSpan(0, 1, 2, 3, 4);

        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", value)
                   .AppendKeyValue("World", (TimeSpan?)value)
                   .AppendKeyValue("Null", (TimeSpan?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""01:02:03.0040000"", ""World"": ""01:02:03.0040000"", ""Null"": null }");
    }

    [Test]
    public void should_format_json_enum()
    {
        _logMessage.Append("Foo")
                   .AppendKeyValue("Hello", DayOfWeek.Friday)
                   .AppendKeyValue("World", (DayOfWeek?)DayOfWeek.Saturday)
                   .AppendKeyValue("Null", (DayOfWeek?)null)
                   .Append("Bar");

        GetFormattedSimple().ShouldEqual(@"FooBar ~~ { ""Hello"": ""Friday"", ""World"": ""Saturday"", ""Null"": null }");
    }

    private string GetFormattedFull()
    {
        var message = new FormattedLogMessage(_bufferLength, ZeroLogConfiguration.Default);
        message.SetMessage(_logMessage);

        var formatter = new DefaultFormatter();
        return formatter.FormatMessage(message).ToString();
    }

    private string GetFormattedSimple()
    {
        var message = new FormattedLogMessage(_bufferLength, ZeroLogConfiguration.Default);
        message.SetMessage(_logMessage);

        var formatter = new DefaultFormatter
        {
            PrefixPattern = string.Empty
        };

        return formatter.FormatMessage(message).ToString().TrimEnd();
    }
}
