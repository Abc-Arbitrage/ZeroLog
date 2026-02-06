using ZeroLog.Appenders;

namespace ZeroLog.Formatting;

/// <summary>
/// The default formatter of appenders derived from <see cref="StreamAppender"/>.
/// Produces human-readable text consisting of:
/// <list type="bullet">
/// <item>A prefix containing information about the logged message. See <see cref="PrefixPattern"/>.</item>
/// <item>The logged message text.</item>
/// <item>The message metadata encoded a JSON string, prefixed by <see cref="JsonSeparator"/> if metadata is present.</item>
/// <item>The associated exception on the next lines.</item>
/// </list>
/// </summary>
/// <remarks>
/// Logging an exception will allocate.
/// </remarks>
public sealed class DefaultFormatter : Formatter
{
    /// <summary>
    /// The writer used for the start of the message.
    /// </summary>
    /// <seealso cref="PrefixPattern"/>
    public PatternWriter MessagePatternWriter { get; init; }

    /// <summary>
    /// The message prefix pattern. Use <see cref="MessagePatternWriter"/> for more customization.
    /// </summary>
    /// <inheritdoc cref="PatternWriter" path="/remarks" />
    /// <seealso cref="MessagePatternWriter"/>
    [PatternWriter.Pattern]
    public string PrefixPattern
    {
        get => MessagePatternWriter.Pattern;
        init => MessagePatternWriter = new PatternWriter(value);
    }

    /// <summary>
    /// The separator to write before the JSON metadata.
    /// </summary>
    public string JsonSeparator { get; init; } = " ~~ ";

    /// <summary>
    /// Initializes a new instance of the default formatter, with the default pattern.
    /// </summary>
    public DefaultFormatter()
        : this(((DefaultFormatter)DefaultStyle.NoColor.Default.Formatter).MessagePatternWriter)
    {
    }

    /// <summary>
    /// Initializes a new instance of the default formatter with a custom pattern.
    /// </summary>
    /// <inheritdoc cref="PatternWriter" path="/remarks" />
    /// <seealso cref="MessagePatternWriter"/>
    public DefaultFormatter([PatternWriter.Pattern] string pattern)
        : this(new PatternWriter(pattern))
    {
    }

    /// <summary>
    /// Initializes a new instance of the default formatter with a custom pattern writer.
    /// </summary>
    public DefaultFormatter(PatternWriter writer)
    {
        MessagePatternWriter = writer;
    }

    /// <inheritdoc cref="PatternWriter.EscapePattern"/>
    public static string EscapePlaceholders(string? value)
        => PatternWriter.EscapePattern(value);

    /// <inheritdoc/>
    protected override void WriteMessage(LoggedMessage message)
    {
        Write(message, MessagePatternWriter);

        if (!MessagePatternWriter.HasMessage) // For backwards compatibility
            Write(message.Message);

        if (message.KeyValues.Count != 0)
        {
            Write(JsonSeparator);
            Write(message.KeyValues);
        }

        if (message.Exception != null)
        {
            WriteLine();

            if (MessagePatternWriter.HasAnsiCodes)
                Write(DefaultStyle.Defaults.Exception);

            // This allocates, but there's no better way to get the details.
            Write(message.Exception.ToString());
        }

        if (MessagePatternWriter.HasAnsiCodes)
            Write(AnsiColorCodes.Reset);

        WriteLine();
    }

    /// <summary>
    /// Returns this formatter with any ANSI color codes removed.
    /// </summary>
    public DefaultFormatter WithoutAnsiColorCodes()
    {
        if (!MessagePatternWriter.HasAnsiCodes && !AnsiColorCodes.HasAnsiCode(JsonSeparator))
            return this;

        return new DefaultFormatter
        {
            MessagePatternWriter = MessagePatternWriter.WithoutAnsiColorCodes(),
            JsonSeparator = AnsiColorCodes.RemoveAnsiCodes(JsonSeparator)
        };
    }

    /// <inheritdoc/>
    public override string ToString()
        => PrefixPattern;
}
