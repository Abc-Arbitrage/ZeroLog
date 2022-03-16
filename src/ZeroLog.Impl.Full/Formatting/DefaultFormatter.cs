namespace ZeroLog.Formatting;

public sealed class DefaultFormatter : Formatter
{
    private readonly PrefixWriter? _prefixWriter;

    public string PrefixPattern
    {
        get => _prefixWriter?.Pattern ?? string.Empty;
        init => _prefixWriter = !string.IsNullOrEmpty(value) ? new PrefixWriter(value) : null;
    }

    public string JsonSeparator { get; init; } = " ~~ ";

    public DefaultFormatter()
    {
        PrefixPattern = "%time - %level - %logger || ";
    }

    protected override void WriteMessage(FormattedLogMessage message)
    {
        if (_prefixWriter != null)
            AdvanceBy(_prefixWriter.WritePrefix(message, GetRemainingBuffer()));

        Write(message.GetMessage());

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
