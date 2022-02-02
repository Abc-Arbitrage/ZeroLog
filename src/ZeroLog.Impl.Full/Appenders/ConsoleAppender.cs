using System;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

public class ConsoleAppender : StreamAppender
{
    private LogLevel _lastLoggedLevel = LogLevel.None;

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
    }

    public override void Flush()
    {
        base.Flush();

        if (ColorOutput)
            Console.ResetColor();

        _lastLoggedLevel = LogLevel.None;
    }

    private void UpdateConsoleColor(FormattedLogMessage message)
    {
        if (message.Level == _lastLoggedLevel)
            return;

        if (_lastLoggedLevel != LogLevel.None)
            base.Flush();

        _lastLoggedLevel = message.Level;

        switch (message.Level)
        {
            case LogLevel.Fatal:
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                break;

            case LogLevel.Error:
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Red;
                break;

            case LogLevel.Warn:
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;

            case LogLevel.Info:
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                break;

            case LogLevel.Debug:
            case LogLevel.Trace:
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;
                break;

            default:
                goto case LogLevel.Info;
        }
    }
}
