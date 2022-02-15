using System;
using System.Globalization;
using System.IO;
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
        Directory.Delete(_appender.FileDirectory, true);
    }

    [Test, RequiresThread]
    public void should_log_to_file()
    {
        _appender = new DateAndSizeRollingFileAppender(_directory)
        {
            PrefixPattern = "%date - %time - %thread - %level - %logger || "
        };

        var logMessage = new LogMessage("Test log message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
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
            PrefixPattern = "",
            MaxFileSizeInBytes = 100
        };

        var logMessage = new LogMessage(new string('a', 50));
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
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

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
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
            PrefixPattern = ""
        };

        var logMessage = new LogMessage("Test message");
        logMessage.Initialize(new Log("TestLog"), LogLevel.Info);

        var formattedMessage = new FormattedLogMessage(logMessage.ToString().Length, ZeroLogConfiguration.Default);
        formattedMessage.SetMessage(logMessage);

        Directory.CreateDirectory(_appender.FileDirectory);
        File.WriteAllLines(Path.Combine(_appender.FileDirectory, "0"), new[] { "File 0" });
        File.WriteAllLines(Path.Combine(_appender.FileDirectory, "1"), new[] { "File 1" });
        File.WriteAllLines(Path.Combine(_appender.FileDirectory, "2"), new[] { "File 2" });

        _appender.WriteMessage(formattedMessage);
        _appender.Dispose();

        File.ReadAllLines(Path.Combine(_appender.FileDirectory, "0")).ShouldEqual(new[] { "File 0" });
        File.ReadAllLines(Path.Combine(_appender.FileDirectory, "1")).ShouldEqual(new[] { "File 1" });
        File.ReadAllLines(Path.Combine(_appender.FileDirectory, "2")).ShouldEqual(new[] { "File 2", logMessage.ToString() });
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
}
