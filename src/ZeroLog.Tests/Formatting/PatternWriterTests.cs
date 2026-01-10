using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Formatting;

[TestFixture]
public class PatternWriterTests
{
    [Test]
    [TestCase("", "")]
    [TestCase("foo", "foo")]
    [TestCase("%%", "%")]
    [TestCase("%date", "2020-01-02")]
    [TestCase("%localDate", "2020-01-01")]
    [TestCase("%time", "03:04:05.0060000")]
    [TestCase("%localTime", "17:04:05.0060000")]
    [TestCase("%level", "INFO")]
    [TestCase("%logger", "Foo.Bar.TestLog")]
    [TestCase("%loggerCompact", "FB.TestLog")]
    [TestCase("%message", "Foo")]
    [TestCase("%exceptionMessage", "Exception message")]
    [TestCase("%exceptionType", "InvalidOperationException")]
    [TestCase("foo %level bar %logger baz", "foo INFO bar Foo.Bar.TestLog baz")]
    [TestCase("%level %level", "INFO INFO")]
    [TestCase("%LEVEL", "INFO")]
    [TestCase("%{level}", "INFO")]
    [TestCase("%{ level  }", "INFO")]
    [TestCase("foo%{level}Bar", "fooINFOBar")]
    [TestCase("foo%{level:6}Bar%{level:5}Baz", "fooINFO  BarINFO Baz")]
    [TestCase("foo%{level}", "fooINFO")]
    [TestCase("foo%level", "fooINFO")]
    [TestCase("%{level}Bar", "INFOBar")]
    [TestCase("%{level}%{logger}", "INFOFoo.Bar.TestLog")]
    [TestCase("%{date:dd MM yyyy}", "02 01 2020")]
    [TestCase("%{localDate:dd MM yyyy HH mm ss}", "01 01 2020 17 04 05")]
    [TestCase("%{date:lol}", "lol")]
    [TestCase("%{time:hh\\:mm}", "03:04")]
    [TestCase("%{level:pad}", "INFO ")]
    [TestCase("%{level:5}", "INFO ")]
    [TestCase("%{level:6}", "INFO  ")]
    [TestCase("%levelColor%level", "\e[97mINFO")]
    [TestCase("%{logger:3}", "Foo.Bar.TestLog")]
    [TestCase("%{logger:18}", "Foo.Bar.TestLog   ")]
    [TestCase("%{loggerCompact:12}", "FB.TestLog  ")]
    [TestCase("%{message:5}", "Foo  ")]
    [TestCase("abc%{column:10}def%{column:15}ghi", "abc       def  ghi")]
    [TestCase("%%level", "%level")]
    [TestCase("%%%level", "%INFO")]
    [TestCase("%%{level}", "%{level}")]
    [TestCase("foo%%{level}bar", "foo%{level}bar")]
    [TestCase("%%%{level}", "%INFO")]
    [TestCase("foo%%%{level}Bar", "foo%INFOBar")]
    [TestCase("%{%}", "%{%}")]
    [TestCase("%{%:%}", "%{%:%}")]
    [TestCase("%%{%}", "%{%}")]
    [TestCase("foo%{resetColor}bar", "foo\e[0mbar")]
    public void should_write_pattern(string pattern, string expectedResult)
    {
        var patternWriter = new PatternWriter(pattern)
        {
            LocalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Etc/GMT+10")
        };

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), LogLevel.Info);
        logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);
        logMessage.Exception = new InvalidOperationException("Exception message");

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual(expectedResult);

        PatternWriter.IsValidPattern(pattern).ShouldBeTrue();
    }

    [Test]
    [TestCase("%{date:\\}")]
    [TestCase("%{localDate:\\}")]
    [TestCase("%{time:\\}")]
    [TestCase("%{localTime:\\}")]
    [TestCase("%{level:-3}")]
    [TestCase("%{level:lol}")]
    [TestCase("%{logger:-3}")]
    [TestCase("%{logger:lol}")]
    [TestCase("%foo")]
    [TestCase("%{foo}")]
    [TestCase("%{foo:bar}")]
    [TestCase("%{newline:5}")]
    [TestCase("%column")]
    [TestCase("%{column}")]
    [TestCase("%{column:foo}")]
    [TestCase("%{column:-3}")]
    [TestCase("%{resetColor:5}")]
    public void should_throw_on_invalid_format(string pattern)
    {
        Assert.Throws<FormatException>(() => _ = new PatternWriter(pattern));
        PatternWriter.IsValidPattern(pattern).ShouldBeFalse();
    }

    [Test]
    [TestCase("")]
    [TestCase("foo")]
    [TestCase("%%")]
    [TestCase("%date")]
    [TestCase("%localDate")]
    [TestCase("%time")]
    [TestCase("%localTime")]
    [TestCase("%thread")]
    [TestCase("%threadId")]
    [TestCase("%threadName")]
    [TestCase("%level")]
    [TestCase("%levelColor")]
    [TestCase("%logger")]
    [TestCase("%loggerCompact")]
    [TestCase("%newline")]
    [TestCase("%resetColor")]
    [TestCase("abc%{column:10}def")]
    [TestCase("foo %level bar %logger baz")]
    [TestCase("%{date:dd MM yyyy HH mm ss}")]
    [TestCase("%{localDate:dd MM yyyy HH mm ss}")]
    [TestCase("%{level:pad}")]
    public void should_not_allocate(string pattern)
    {
        var patternWriter = new PatternWriter(pattern);

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), LogLevel.Info);
        logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);

        var buffer = new char[256];
        var formattedLogMessage = new LoggedMessage(256, ZeroLogConfiguration.Default);
        formattedLogMessage.SetMessage(logMessage);

        GcTester.ShouldNotAllocate(() => patternWriter.Write(formattedLogMessage, buffer, out _));

        PatternWriter.IsValidPattern(pattern).ShouldBeTrue();
    }

    [Test]
    [TestCase(LogLevel.Trace, "[] [           ]")]
    [TestCase(LogLevel.Debug, "[DEbug] [DEbug      ]")]
    [TestCase(LogLevel.Info, "[InFo] [InFo       ]")]
    [TestCase(LogLevel.Warn, "[WARN] [WARN       ]")]
    [TestCase(LogLevel.Error, "[ERROR OMG] [ERROR OMG  ]")]
    [TestCase(LogLevel.Fatal, "[CRITICAL!!!] [CRITICAL!!!]")]
    public void should_customize_levels(LogLevel level, string expectedResult)
    {
        var patternWriter = new PatternWriter("[%level] [%{level:pad}]")
        {
            LogLevels = new(null!, "DEbug", "InFo", "WARN", "ERROR OMG", "CRITICAL!!!")
        };

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), level);
        logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual(expectedResult);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(42)]
    public void should_handle_invalid_level_values(int level)
    {
        var patternWriter = new PatternWriter("[%level] [%{level:pad}]")
        {
            LogLevels = new(null!, "DEbug", "InFo", "WARN", "ERROR OMG", "CRITICAL!!!")
        };

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), (LogLevel)level);
        logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual("[] []");
    }

    [Test]
    [TestCase(-1, "[] []")]
    [TestCase(0, "[TRACE] [TRACE]")]
    [TestCase(1, "[DEBUG] [DEBUG]")]
    [TestCase(2, "[INFO] [INFO ]")]
    [TestCase(3, "[WARN] [WARN ]")]
    [TestCase(4, "[ERROR] [ERROR]")]
    [TestCase(5, "[FATAL] [FATAL]")]
    [TestCase(6, "[] []")]
    public void should_handle_invalid_level_values_on_default_instance(int level, string expected)
    {
        var patternWriter = new PatternWriter("[%level] [%{level:pad}]")
        {
            LogLevels = default
        };

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), (LogLevel)level);
        logMessage.Timestamp = new DateTime(2020, 01, 02, 03, 04, 05, 06);

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual(expected);
    }

    [Test]
    [TestCase(LogLevel.Trace, "[\e[30m]")]
    [TestCase(LogLevel.Debug, "[\e[91m]")]
    [TestCase(LogLevel.Info, "[\e[92m]")]
    [TestCase(LogLevel.Warn, "[\e[93m]")]
    [TestCase(LogLevel.Error, "[\e[94m]")]
    [TestCase(LogLevel.Fatal, "[\e[95m]")]
    public void should_customize_level_colors(LogLevel level, string expectedResult)
    {
        var patternWriter = new PatternWriter("[%levelColor]")
        {
            LogLevelColors = new(
                ConsoleColor.Black,
                ConsoleColor.Red,
                ConsoleColor.Green,
                ConsoleColor.Yellow,
                ConsoleColor.Blue,
                ConsoleColor.Magenta
            )
        };

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), level);

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual(expectedResult);
    }

    [Test]
    [TestCase("%thread")]
    [TestCase("%threadName")]
    public void should_write_thread_name(string format)
    {
        RunOnCustomThread(() =>
        {
            Thread.CurrentThread.Name = "Hello";

            var patternWriter = new PatternWriter($"{format} world!");

            var logMessage = new LogMessage("Foo");
            logMessage.Initialize(null, LogLevel.Info);

            var result = GetResult(patternWriter, logMessage);
            result.ShouldEqual("Hello world!");
        });
    }

    [Test]
    [TestCase("%thread")]
    [TestCase("%threadId")]
    public void should_write_thread_id(string format)
    {
        RunOnCustomThread(() =>
        {
            var patternWriter = new PatternWriter(format);

            var logMessage = new LogMessage("Foo");
            logMessage.Initialize(null, LogLevel.Info);

            var result = GetResult(patternWriter, logMessage);
            result.ShouldEqual(Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
        });
    }

    [Test]
    [TestCase("%thread")]
    [TestCase("%threadId")]
    public void should_write_zero_when_no_thread_provided(string format)
    {
        RunOnCustomThread(() =>
        {
            var patternWriter = new PatternWriter(format);

            var logMessage = new LogMessage("Foo");

            var result = GetResult(patternWriter, logMessage);
            result.ShouldEqual("0");
        });
    }

    [Test]
    public void should_not_write_anything_for_unnamed_thread_with_threadName()
    {
        RunOnCustomThread(() =>
        {
            var patternWriter = new PatternWriter("foo%{threadName}bar");

            var logMessage = new LogMessage("Foo");

            var result = GetResult(patternWriter, logMessage);
            result.ShouldEqual("foobar");
        });
    }

    [Test]
    public void should_write_newline()
    {
        var patternWriter = new PatternWriter("[%{newline}]");
        var logMessage = new LogMessage("Foo");

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual($"[{Environment.NewLine}]");
    }

    [Test]
    [TestCase(null, "")]
    [TestCase("", "")]
    [TestCase("foo", "foo")]
    [TestCase("%foo", "%%foo")]
    [TestCase("%foo %%bar", "%%foo %%%%bar")]
    public void should_escape_pattern(string pattern, string expectedResult)
        => PatternWriter.EscapePattern(pattern).ShouldEqual(expectedResult);

    [Test]
    [TestCase("", "", "")]
    [TestCase("foo", "foo", "foo")]
    [TestCase("foo%{resetColor}bar", "foobar", "foobar")]
    [TestCase("foo%{levelColor}bar", "foobar", "foobar")]
    [TestCase("foo%{resetColor}bar%{levelColor}%{resetColor}baz", "foobarbaz", "foobarbaz")]
    [TestCase("foo%{level}bar", "foo%{level}bar", "fooINFObar")]
    public void should_remove_ansi_codes(string pattern, string expectedPattern, string expectedResult)
    {
        var initialPatternWriter = new PatternWriter(pattern);
        initialPatternWriter.Pattern.ShouldEqual(pattern);
        initialPatternWriter.LogLevelColors[LogLevel.Info].ShouldContain("\e");

        var patternWriter = initialPatternWriter.WithoutAnsiColorCodes();
        patternWriter.HasAnsiCodes.ShouldBeFalse();
        patternWriter.Pattern.ShouldEqual(expectedPattern);
        patternWriter.LogLevels[LogLevel.Info].ShouldEqual("INFO");
        patternWriter.LogLevelColors[LogLevel.Info].ShouldEqual("");
        AnsiColorCodes.HasAnsiCode(patternWriter.Pattern).ShouldBeFalse();

        var logMessage = new LogMessage("Foo");
        logMessage.Initialize(new Log("Foo.Bar.TestLog"), LogLevel.Info);

        var result = GetResult(patternWriter, logMessage);
        result.ShouldEqual(expectedResult);
    }

    private static string GetResult(PatternWriter patternWriter, LogMessage logMessage)
    {
        var buffer = new char[256];
        var formattedLogMessage = new LoggedMessage(256, ZeroLogConfiguration.Default);
        formattedLogMessage.SetMessage(logMessage);
        patternWriter.Write(formattedLogMessage, buffer, out var charsWritten);
        return buffer.AsSpan(0, charsWritten).ToString();
    }

    private static void RunOnCustomThread(Action action)
    {
        // NUnit assigns the thread a name when using [RequiresThread],
        // so create a custom thread to avoid that.

        Exception threadException = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                threadException = ex;
            }
        });

        thread.Start();
        thread.Join();

        threadException.ShouldBeNull();
    }
}
