using System;

namespace ZeroLog.Appenders;

public class ConsoleAppender : StreamAppender
{
    public const string DefaultPrefixPattern = "%time - %level - %logger || ";

    public ConsoleAppender()
        : this(DefaultPrefixPattern)
    {
    }

    public ConsoleAppender(string prefixPattern)
        : base(prefixPattern)
    {
        _stream = Console.OpenStandardOutput();
        _encoding = Console.OutputEncoding;
    }

    public override void WriteMessage(FormattedLogMessage message)
    {
        Console.BackgroundColor = ConsoleColor.Black;

        switch (message.Message.Level)
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

        base.WriteMessage(message);
        Flush();
    }
}
