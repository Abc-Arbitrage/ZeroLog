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
    private readonly char[] _messageBuffer;
    private int _messageLength;
    private ZeroLogConfiguration _config;

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
    /// The name of the logger which logged this message, with its namespace shortened to the initials.
    /// </summary>
    public string? LoggerCompactName => _message.Logger?.CompactName;

    internal Log? Logger => _message.Logger;

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
        KeyValues = new KeyValueList(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    private LoggedMessage(LoggedMessage other)
    {
        _config = other._config;

        _messageBuffer = other._messageBuffer.AsSpan(0, other._messageLength).ToArray();
        _messageLength = other._messageLength;

        KeyValues = new KeyValueList(other.KeyValues);
        _message = other._message.CloneMetadata();
    }

    internal void SetMessage(LogMessage message)
    {
        _message = message;

        try
        {
#if DEBUG
            _messageBuffer.AsSpan().Fill((char)0);
#endif

            _messageLength = _message.WriteTo(_messageBuffer, _config, LogMessage.FormatType.Formatted, KeyValues);
        }
        catch (Exception ex)
        {
            HandleFormattingError(ex);
        }
    }

    internal void UpdateConfiguration(ZeroLogConfiguration config)
    {
        _config = config;
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

    /// <summary>
    /// Returns a clone of this message.
    /// </summary>
    /// <returns>
    /// <para>
    /// The instance of <see cref="LoggedMessage"/> which is passed to the appenders
    /// is a singleton in order to avoid allocations, and is overwritten for each logged message.
    /// Cloning a message can be useful to capture logged data in unit tests for instance.
    /// </para>
    /// <para>
    /// This method allocates.
    /// </para>
    /// </returns>
    public LoggedMessage Clone()
        => new(this);

    /// <summary>
    /// Creates a <see cref="LoggedMessage"/> for unit testing purposes.
    /// </summary>
    /// <remarks>
    /// Do not modify the provided <paramref name="message"/> while using the returned <see cref="LoggedMessage"/>.
    /// </remarks>
    /// <param name="message">The message to convert.</param>
    /// <param name="config">The configuration to use. Uses <see cref="ZeroLogConfiguration.Default"/> by default.</param>
    /// <returns>A standalone log message.</returns>
    /// <seealso cref="LogMessage.CreateTestMessage(ZeroLog.LogLevel,int,int)"/>
    public static LoggedMessage CreateTestMessage(LogMessage message, ZeroLogConfiguration? config = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        var loggedMessage = new LoggedMessage(LogManager.OutputBufferSize, config ?? ZeroLogConfiguration.Default);
        loggedMessage.SetMessage(message);
        return loggedMessage;
    }
}
