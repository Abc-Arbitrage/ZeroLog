using System;

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
            case Level.Fatal:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                break;

            case Level.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case Level.Warn:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case Level.Info:
                Console.ForegroundColor = ConsoleColor.White;
                break;

            case Level.Debug:
            case Level.Trace:
                Console.ForegroundColor = ConsoleColor.Gray;
                break;

            default:
                Console.ForegroundColor = ConsoleColor.White;
                break;
        }
    }
}
