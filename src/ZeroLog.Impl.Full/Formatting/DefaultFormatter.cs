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
    private static readonly PrefixWriter _defaultPrefixWriter = new("%time - %level - %logger || ");

    private readonly PrefixWriter? _prefixWriter = _defaultPrefixWriter;

    /// <summary>
    /// The message prefix pattern. This is logged before the message text.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The pattern is a string containing placeholders:
    /// <list type="table">
    /// <item><term><c>%date</c></term><description>The message UTC date (default format: <c>yyyy-MM-dd</c>).</description></item>
    /// <item><term><c>%time</c></term><description>The message UTC timestamp (default format: <c>hh\:mm\:ss\.fffffff</c>).</description></item>
    /// <item><term><c>%thread</c></term><description>The thread name (or ID) which logged the message.</description></item>
    /// <item><term><c>%level</c></term><description>The log level in uppercase (specify the <c>pad</c> format to make each level 5 characters wide).</description></item>
    /// <item><term><c>%logger</c></term><description>The logger name.</description></item>
    /// <item><term><c>%newline</c></term><description>Equivalent to <c>Environment.NewLine</c>.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Placeholders can be surrounded with braces and specify an optional format string:
    /// <c>%{date:yyyy-MM-dd HH:mm:ss}</c> for instance.
    /// </para>
    /// <para>
    /// Format strings can also be used to set a minimum field length: <c>%{logger:20}</c> will always be at least 20 characters wide.
    /// </para>
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
