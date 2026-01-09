using System;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// An appender which logs to the standard output.
/// </summary>
public class ConsoleAppender : StreamAppender
{
    private LogLevel _lastLoggedLevel = LogLevel.None;

    /// <summary>
    /// Defines whether messages should be colored.
    /// </summary>
    /// <remarks>
    /// True by default when the standard output is not redirected.
    /// </remarks>
    public bool ColorOutput { get; init; }

    /// <summary>
    /// Initializes a new instance of the console appender.
    /// </summary>
    public ConsoleAppender()
    {
        Stream = Console.OpenStandardOutput();
        Encoding = Console.OutputEncoding;
        ColorOutput = AnsiColorCodes.UseByDefault;
    }

    /// <inheritdoc/>
    public override void WriteMessage(LoggedMessage message)
    {
        if (ColorOutput)
            UpdateConsoleColor(message);

        base.WriteMessage(message);
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        base.Flush();

        if (ColorOutput)
            Console.ResetColor();

        _lastLoggedLevel = LogLevel.None;
    }

    private void UpdateConsoleColor(LoggedMessage message)
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
