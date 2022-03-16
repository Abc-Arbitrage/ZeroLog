using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Configuration;

namespace ZeroLog.Formatting;

/// <summary>
/// Represents a logged message.
/// </summary>
public sealed class LoggedMessage
{
    private readonly ZeroLogConfiguration _config;
    private readonly char[] _messageBuffer;
    private int _messageLength;

    private LogMessage _message = LogMessage.Empty;

    /// <inheritdoc cref="LogMessage.Level"/>
    public LogLevel Level => _message.Level;

    /// <inheritdoc cref="LogMessage.Timestamp"/>
    public DateTime Timestamp => _message.Timestamp;

    /// <inheritdoc cref="LogMessage.Thread"/>
    public Thread? Thread => _message.Thread;

    /// <inheritdoc cref="LogMessage.Exception"/>
    public Exception? Exception => _message.Exception;

    /// <summary>
    /// The name of the logger which logged this message.
    /// </summary>
    public string? LoggerName => _message.Logger?.Name;

    /// <summary>
    /// The logged message text.
    /// </summary>
    public ReadOnlySpan<char> Message => _messageBuffer.AsSpan(0, _messageLength);

    /// <summary>
    /// The logged message metadata as a list of key/value pairs.
    /// </summary>
    public KeyValueList KeyValues { get; }

    internal LoggedMessage(int bufferSize, ZeroLogConfiguration config)
    {
        _config = config;
        _messageBuffer = GC.AllocateUninitializedArray<char>(bufferSize);
        KeyValues = new(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    internal void SetMessage(LogMessage message)
    {
        _message = message;

        try
        {
            _messageLength = _message.WriteTo(_messageBuffer, _config, LogMessage.FormatType.Formatted, KeyValues);
        }
        catch (Exception ex)
        {
            HandleFormattingError(ex);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void HandleFormattingError(Exception ex)
    {
        try
        {
            var builder = new CharBufferBuilder(_messageBuffer);
            builder.TryAppendPartial("An error occurred during formatting: ");
            builder.TryAppendPartial(ex.Message);
            builder.TryAppendPartial(" - Unformatted message: ");

            var length = _message.WriteTo(builder.GetRemainingBuffer(), _config, LogMessage.FormatType.Unformatted, null);
            _messageLength = builder.Length + length;
        }
        catch
        {
            var builder = new CharBufferBuilder(_messageBuffer);
            builder.TryAppendPartial("An error occurred during formatting: ");
            builder.TryAppendPartial(ex.Message);
            _messageLength = builder.Length;
        }
    }

    /// <summary>
    /// Returns the logged message text.
    /// </summary>
    public override string ToString()
        => Message.ToString();
}
