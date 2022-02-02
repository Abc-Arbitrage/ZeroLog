using System;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public class ConsoleAppender : StreamAppender
{
    public bool ColorOutput { get; set; }

    public ConsoleAppender()
    {
        PrefixPattern = "%time - %level - %logger || ";

        Stream = Console.OpenStandardOutput();
        Encoding = Console.OutputEncoding;
        ColorOutput = !Console.IsOutputRedirected;
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        if (ColorOutput)
            UpdateConsoleColor(message);

        base.WriteMessage(message);
        Flush();
    }

    private static void UpdateConsoleColor(FormattedLogMessage message)
    {
        Console.BackgroundColor = ConsoleColor.Black;

        switch (message.Level)
        {
            case LogLevel.Fatal:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                break;

            case LogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case LogLevel.Warn:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case LogLevel.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;

            case LogLevel.Debug:
            case LogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;

            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }
    }
}
