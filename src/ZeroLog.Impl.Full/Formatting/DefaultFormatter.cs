using ZeroLog.Appenders;
using C = ZeroLog.Formatting.AnsiColorCodes;

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
    internal static readonly PatternWriter DefaultColoredPrefixWriter = new(
        $"%resetColor{C.DefaultTimestamp}%time%resetColor - %levelColor%{{level:pad}}%resetColor - {C.DefaultLogger}%logger%resetColor || {C.DefaultMessage}%message%resetColor"
    );

    internal static readonly PatternWriter DefaultPrefixWriter = DefaultColoredPrefixWriter.WithoutAnsiColorCodes();

    /// <summary>
    /// The writer used for the start of the message.
    /// </summary>
    /// <seealso cref="PrefixPattern"/>
    public PatternWriter PrefixPatternWriter { get; init; } = DefaultPrefixWriter;

    /// <summary>
    /// The message prefix pattern. Use <see cref="PrefixPatternWriter"/> for more customization.
    /// </summary>
    /// <inheritdoc cref="PatternWriter" path="/remarks" />
    /// <seealso cref="PrefixPatternWriter"/>
    public string PrefixPattern
    {
        get => PrefixPatternWriter.Pattern;
        init => PrefixPatternWriter = new PatternWriter(value);
    }

    /// <summary>
    /// The separator to write before the JSON metadata.
    /// </summary>
    public string JsonSeparator { get; init; } = " ~~ ";

    /// <inheritdoc cref="PatternWriter.EscapePattern"/>
    public static string EscapePlaceholders(string? value)
        => PatternWriter.EscapePattern(value);

    /// <inheritdoc/>
    protected override void WriteMessage(LoggedMessage message)
    {
        Write(message, PrefixPatternWriter);

        if (!PrefixPatternWriter.HasMessage) // For backwards compatibility
            Write(message.Message);

        if (message.KeyValues.Count != 0)
        {
            Write(JsonSeparator);
            Write(message.KeyValues);
        }

        WriteLine();

        if (message.Exception != null)
        {
            if (PrefixPatternWriter.HasAnsiCodes)
                Write(C.ForegroundBrightRed);

            // This allocates, but there's no better way to get the details.
            WriteLine(message.Exception.ToString());

            if (PrefixPatternWriter.HasAnsiCodes)
                Write(C.Reset);
        }
    }

    internal DefaultFormatter WithoutAnsiColorCodes()
    {
        if (!PrefixPatternWriter.HasAnsiCodes && !AnsiColorCodes.HasAnsiCode(JsonSeparator))
            return this;

        return new DefaultFormatter
        {
            PrefixPatternWriter = PrefixPatternWriter.WithoutAnsiColorCodes(),
            JsonSeparator = AnsiColorCodes.RemoveAnsiCodes(JsonSeparator)
        };
    }
}
