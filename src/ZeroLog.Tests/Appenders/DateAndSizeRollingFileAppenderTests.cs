using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
public class DateAndSizeRollingFileAppenderTests
{
    private DateAndSizeRollingFileAppender _appender;
    private string _directory;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
    }

    [TearDown]
    public void Teardown()
    {
        _appender.Dispose();
        Directory.Delete(_appender.Directory, true);
    }

    [Test, RequiresThread]
    public void should_log_to_file()
    {
        _appender = new DateAndSizeRollingFileAppender(_directory)
        {
            Formatter = new DefaultFormatter
            {
                PrefixPattern = "%date - %time - %thread - %level - %logger || "
            }
        };

        var logMessage = new LogMessage("Test log message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        _appender.WriteMessage(formattedMessage);
        _appender.WriteMessage(formattedMessage);
        _appender.Flush();

        using var reader = new StreamReader(File.Open(_appender.Stream.ShouldBe<FileStream>().Name.ShouldNotBeNull(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite), _appender.Encoding);
        var text = reader.ReadToEnd();

        var expectedLine = $"{logMessage.Timestamp.Date:yyyy-MM-dd} - {logMessage.Timestamp.TimeOfDay:hh\\:mm\\:ss\\.fffffff} - {Thread.CurrentThread.ManagedThreadId} - INFO - TestLog || {logMessage}";
        text.ShouldEqual(expectedLine + Environment.NewLine + expectedLine + Environment.NewLine);
    }

    [Test]
    public void should_roll_file_by_number()
    {
        _appender = new FileNumberPatternAppender(_directory)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" },
            MaxFileSizeInBytes = 100
        };

        var logMessage = new LogMessage(new string('a', 50));
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual("0");

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual("0");

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual("1");

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual("1");

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual("2");
    }

    [Test]
    public void should_roll_file_by_date()
    {
        _appender = new DatePatternAppender(_directory);

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual(logMessage.Timestamp.ToString("yyyyMMdd"));

        logMessage.Timestamp = logMessage.Timestamp.AddDays(1);

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual(logMessage.Timestamp.ToString("yyyyMMdd"));

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual(logMessage.Timestamp.ToString("yyyyMMdd"));

        logMessage.Timestamp = logMessage.Timestamp.AddDays(1);

        _appender.WriteMessage(formattedMessage);
        Path.GetFileName(_appender.Stream.ShouldBe<FileStream>().Name).ShouldEqual(logMessage.Timestamp.ToString("yyyyMMdd"));
    }

    [Test]
    public void should_append_to_last_file()
    {
        _appender = new FileNumberPatternAppender(_directory)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" }
        };

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        Directory.CreateDirectory(_appender.Directory);
        File.WriteAllLines(Path.Combine(_appender.Directory, "0"), new[] { "File 0" });
        File.WriteAllLines(Path.Combine(_appender.Directory, "1"), new[] { "File 1" });
        File.WriteAllLines(Path.Combine(_appender.Directory, "2"), new[] { "File 2" });

        _appender.WriteMessage(formattedMessage);
        _appender.Dispose();

        File.ReadAllLines(Path.Combine(_appender.Directory, "0")).ShouldEqual(new[] { "File 0" });
        File.ReadAllLines(Path.Combine(_appender.Directory, "1")).ShouldEqual(new[] { "File 1" });
        File.ReadAllLines(Path.Combine(_appender.Directory, "2")).ShouldEqual(new[] { "File 2", logMessage.ToString() });
    }

    [Test]
    public void should_create_new_file_if_appending_to_the_last_one_fails()
    {
        _appender = new FileNumberPatternAppender(_directory)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" }
        };

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        Directory.CreateDirectory(_appender.Directory);
        File.WriteAllLines(Path.Combine(_appender.Directory, "0"), new[] { "File 0" });
        File.WriteAllLines(Path.Combine(_appender.Directory, "1"), new[] { "File 1" });
        File.WriteAllLines(Path.Combine(_appender.Directory, "2"), new[] { "File 2" });

        using (File.OpenHandle(Path.Combine(_appender.Directory, "2"), FileMode.Append, FileAccess.Write, FileShare.None))
        {
            _appender.WriteMessage(formattedMessage);
            _appender.Dispose();
        }

        File.ReadAllLines(Path.Combine(_appender.Directory, "0")).ShouldEqual(new[] { "File 0" });
        File.ReadAllLines(Path.Combine(_appender.Directory, "1")).ShouldEqual(new[] { "File 1" });
        File.ReadAllLines(Path.Combine(_appender.Directory, "2")).ShouldEqual(new[] { "File 2" });
        File.ReadAllLines(Path.Combine(_appender.Directory, "3")).ShouldEqual(new[] { logMessage.ToString() });
    }

    [Test]
    public void should_handle_constant_file_name()
    {
        _appender = new ConstantFileNameAppender(_directory)
        {
            Formatter = new DefaultFormatter { PrefixPattern = "" },
            MaxFileSizeInBytes = 2
        };

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        Directory.CreateDirectory(_appender.Directory);
        File.WriteAllLines(Path.Combine(_appender.Directory, ConstantFileNameAppender.FileName), new[] { "First line" });

        _appender.WriteMessage(formattedMessage);
        _appender.WriteMessage(formattedMessage);

        logMessage.Timestamp = logMessage.Timestamp.AddDays(1);
        _appender.WriteMessage(formattedMessage);

        logMessage.Timestamp = logMessage.Timestamp.AddDays(1);
        _appender.WriteMessage(formattedMessage);

        _appender.Dispose();

        File.ReadAllLines(Path.Combine(_appender.Directory, ConstantFileNameAppender.FileName)).ShouldEqual(new[] { "First line", "Test message", "Test message", "Test message", "Test message" });
    }

    [Test]
    public void should_supply_default_file_name_pattern()
    {
        _appender = new DateAndSizeRollingFileAppender(_directory)
        {
            FileNamePrefix = "Foo",
            FileExtension = "bar"
        };

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);
        logMessage.Timestamp = new DateTime(2022, 02, 15);

        var formattedMessage = new LoggedMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        _appender.WriteMessage(formattedMessage);

        Directory.GetFiles(_appender.Directory)
                 .Select(Path.GetFileName)
                 .ShouldEqual(new[] { "Foo.20220215.000.bar" });
    }

    private class FileNumberPatternAppender : DateAndSizeRollingFileAppender
    {
        public FileNumberPatternAppender(string directory)
            : base(directory)
        {
        }

        protected override string GetFileName(DateOnly date, int number)
            => number.ToString(CultureInfo.InvariantCulture);
    }

    private class DatePatternAppender : DateAndSizeRollingFileAppender
    {
        public DatePatternAppender(string directory)
            : base(directory)
        {
        }

        protected override string GetFileName(DateOnly date, int number)
            => date.ToString("yyyyMMdd");
    }

    private class ConstantFileNameAppender : DateAndSizeRollingFileAppender
    {
        public const string FileName = "Foo.log";

        public ConstantFileNameAppender(string directory)
            : base(directory)
        {
        }

        protected override string GetFileName(DateOnly date, int number)
            => FileName;
    }
}
