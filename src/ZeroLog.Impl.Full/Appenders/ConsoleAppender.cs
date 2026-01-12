using System;
using ZeroLog.Formatting;

namespace ZeroLog.Appenders;

/// <summary>
/// An appender which logs to the standard output.
/// </summary>
public class ConsoleAppender : StreamAppender
{
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

        Formatter = DefaultStyle.Colored.Default.Formatter;
    }

    /// <summary>
    /// Initializes a new instance of the console appender from one of the default styles.
    /// </summary>
    public ConsoleAppender(DefaultStyle style)
        : this()
    {
        Formatter = style.Formatter;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        if (!ColorOutput && Formatter is DefaultFormatter defaultFormatter)
            Formatter = defaultFormatter.WithoutAnsiColorCodes();
    }
}
