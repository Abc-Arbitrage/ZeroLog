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
    private static readonly PatternWriter _defaultPrefixWriter = new("%time - %level - %logger || ");

    private readonly PatternWriter? _prefixWriter = _defaultPrefixWriter;

    /// <summary>
    /// The message prefix pattern. This is logged before the message text.
    /// </summary>
    /// <inheritdoc cref="PatternWriter" path="/remarks" />
    public string PrefixPattern
    {
        get => _prefixWriter?.Pattern ?? string.Empty;
        init => _prefixWriter = !string.IsNullOrEmpty(value) ? new PatternWriter(value) : null;
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
        Write(message, _prefixWriter);
        Write(message.Message);

        if (message.KeyValues.Count != 0)
        {
            Write(JsonSeparator);
            Write(message.KeyValues);
        }

        WriteLine();

        if (message.Exception != null)
        {
            // This allocates, but there's no better way to get the details.
            WriteLine(message.Exception.ToString());
        }
    }
}
