using System;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using ZeroLog.Appenders;
using ZeroLog.Configuration;
using ZeroLog.Formatting;

namespace ZeroLog.Benchmarks.Logging;

[ShortRunJob]
public class StreamAppenderBenchmarks
{
    private readonly Appender _standardFormatterAppender = new(false);
    private readonly Appender _utf8FormatterAppender = new(true);
    private readonly LoggedMessage _loggedMessage = new(1024, ZeroLogConfiguration.CreateTestConfiguration());
    private readonly LogMessage _message;

    public StreamAppenderBenchmarks()
    {
        _message = LogMessage.CreateTestMessage(LogLevel.Info, 1024, 32);
        _message.Append("Hello, ").Append("World! ").Append(42).AppendEnum(DayOfWeek.Friday).Append(new DateTime(2023, 01, 01)).Append(1024);
    }

    [Benchmark(Baseline = true)]
    public void StandardFormatter()
    {
        _loggedMessage.SetMessage(_message);
        _standardFormatterAppender.WriteMessage(_loggedMessage);
    }

    [Benchmark]
    public void Utf8Formatter()
    {
        _loggedMessage.SetMessage(_message);
        _utf8FormatterAppender.WriteMessage(_loggedMessage);
    }

    private class Appender : StreamAppender
    {
        public Appender(bool utf8Formatter)
        {
            AllowUtf8Formatter = utf8Formatter;
            Stream = Stream.Null;
            Encoding = Encoding.UTF8;
        }
    }
}
