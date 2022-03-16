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
public sealed class DefaultFormatter : Formatter
{
    private static readonly PrefixWriter _defaultPrefixWriter = new("%time - %level - %logger || ");

    private readonly PrefixWriter? _prefixWriter = _defaultPrefixWriter;

    /// <summary>
    /// The message prefix pattern. This is logged before the message text.
    /// </summary>
    /// <remarks>
    /// The pattern is a string containing placeholders:
    /// <list type="table">
    /// <item><term><c>%date</c></term><description>The message date in the <c>yyyy-MM-dd</c> format.</description></item>
    /// <item><term><c>%time</c></term><description>The message UTC timestamp in the <c>hh:mm:ss.fffffff</c> format.</description></item>
    /// <item><term><c>%thread</c></term><description>The thread name (or ID) which logged the message.</description></item>
    /// <item><term><c>%level</c></term><description>The log level in uppercase.</description></item>
    /// <item><term><c>%logger</c></term><description>The logger name.</description></item>
    /// </list>
    /// </remarks>
    public string PrefixPattern
    {
        get => _prefixWriter?.Pattern ?? string.Empty;
        init => _prefixWriter = !string.IsNullOrEmpty(value) ? new PrefixWriter(value) : null;
    }

    /// <summary>
    /// The separator to write before the JSON metadata.
    /// </summary>
    public string JsonSeparator { get; init; } = " ~~ ";

    /// <inheritdoc/>
    protected override void WriteMessage(LoggedMessage message)
    {
        if (_prefixWriter != null)
        {
            _prefixWriter.WritePrefix(message, GetRemainingBuffer(), out var charsWritten);
            AdvanceBy(charsWritten);
        }

        Write(message.Message);

        if (message.KeyValues.Count != 0)
        {
            Write(JsonSeparator);

            JsonWriter.WriteJsonToStringBuffer(message.KeyValues, GetRemainingBuffer(), out var charsWritten);
            AdvanceBy(charsWritten);
        }

        WriteLine();

        if (message.Exception != null)
        {
            // This allocates, but there's no better way to get the details.
            WriteLine(message.Exception.ToString());
        }
    }
}
