using System.IO;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Formatting;
using ZeroLog.Tests.Support;

namespace ZeroLog.Tests.Appenders;

[TestFixture]
public class ConsoleAppenderTests
{
    [Test]
    public void should_append_colors_by_default_when_requested([Values] bool colors)
    {
        var stream = new MemoryStream();

        var appender = new ConsoleAppender
        {
            Stream = stream,
            ColorOutput = colors
        };

        appender.Initialize();
        appender.WriteMessage(LoggedMessage.CreateTestMessage(new LogMessage("Hello")));
        appender.Flush();

        var result = appender.Encoding.GetString(stream.ToArray());
        AnsiColorCodes.HasAnsiCode(result).ShouldEqual(colors);
    }
}
