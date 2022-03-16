namespace ZeroLog.Formatting;

public sealed class DefaultFormatter : Formatter
{
    private static readonly PrefixWriter _defaultPrefixWriter = new("%time - %level - %logger || ");

    private readonly PrefixWriter? _prefixWriter = _defaultPrefixWriter;

    public string PrefixPattern
    {
        get => _prefixWriter?.Pattern ?? string.Empty;
        init => _prefixWriter = !string.IsNullOrEmpty(value) ? new PrefixWriter(value) : null;
    }

    public string JsonSeparator { get; init; } = " ~~ ";

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
