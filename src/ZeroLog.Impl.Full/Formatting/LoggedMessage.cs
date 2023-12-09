using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using ZeroLog.Configuration;

namespace ZeroLog.Formatting;

/// <summary>
/// Represents a logged message.
/// </summary>
public sealed class LoggedMessage
{
    private readonly char[] _charBuffer;
    private readonly byte[] _byteBuffer;
    private readonly KeyValueList _keyValues;

    private int _charBufferLength;
    private int _byteBufferLength;

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

    internal Log? Logger => _message.Logger;

    /// <summary>
    /// The logged message text.
    /// </summary>
    public ReadOnlySpan<char> Message => GetMessageUtf16();

    /// <summary>
    /// The logged message text, encoded in UTF-8.
    /// </summary>
    public ReadOnlySpan<byte> MessageUtf8 => GetMessageUtf8();

    /// <summary>
    /// The logged message metadata as a list of key/value pairs.
    /// </summary>
    public KeyValueList KeyValues => GetKeyValues();

    internal LoggedMessage(int bufferSize, ZeroLogConfiguration config)
    {
        _config = config;
        _charBuffer = GC.AllocateUninitializedArray<char>(bufferSize);
        _byteBuffer = GC.AllocateUninitializedArray<byte>(bufferSize * Utf8Formatter.MaxUtf8BytesPerChar);

        _keyValues = new KeyValueList(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    private LoggedMessage(LoggedMessage other)
    {
        _config = other._config;

        _charBuffer = other.Message.ToArray();
        _charBufferLength = other._charBufferLength;

        _byteBuffer = other.MessageUtf8.ToArray();
        _byteBufferLength = other._byteBufferLength;

        _keyValues = new KeyValueList(other.KeyValues);
        _message = other._message.CloneMetadata();
    }

    internal void SetMessage(LogMessage message)
    {
        _message = message;
        _charBufferLength = -1;
        _byteBufferLength = -1;
        _keyValues.Clear();

#if DEBUG
        _charBuffer.AsSpan().Clear();
        _byteBuffer.AsSpan().Clear();
#endif
    }

    private ReadOnlySpan<char> GetMessageUtf16()
    {
        if (_charBufferLength < 0)
            FormatMessage();

        return _charBuffer.AsSpan(0, _charBufferLength);

        [MethodImpl(MethodImplOptions.NoInlining)]
        void FormatMessage()
        {
            try
            {
                _charBufferLength = _byteBufferLength >= 0
                    ? Encoding.UTF8.GetChars(_byteBuffer.AsSpan(0, _byteBufferLength), _charBuffer)
                    : _message.WriteTo(_charBuffer, _config, LogMessage.FormatType.Formatted, _keyValues);
            }
            catch (Exception ex)
            {
                HandleFormattingError(ex);
            }
        }
    }

    private ReadOnlySpan<byte> GetMessageUtf8()
    {
        if (_byteBufferLength < 0)
            FormatMessage();

        return _byteBuffer.AsSpan(0, _byteBufferLength);

        [MethodImpl(MethodImplOptions.NoInlining)]
        void FormatMessage()
        {
            try
            {
#if NET8_0_OR_GREATER
                _byteBufferLength = _charBufferLength >= 0
                    ? Encoding.UTF8.GetBytes(_charBuffer.AsSpan(0, _charBufferLength), _byteBuffer)
                    : _message.WriteTo(_byteBuffer, _config, LogMessage.FormatType.Formatted, _keyValues);
#else
                _byteBufferLength = Encoding.UTF8.GetBytes(GetMessageUtf16(), _byteBuffer);
#endif
            }
            catch (Exception ex)
            {
                HandleFormattingError(ex);
                _byteBufferLength = Encoding.UTF8.GetBytes(_charBuffer.AsSpan(0, _charBufferLength), _byteBuffer);
            }
        }
    }

    private KeyValueList GetKeyValues()
    {
        if (_charBufferLength < 0 && _byteBufferLength < 0)
            GetMessageUtf8();

        return _keyValues;
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
            var builder = new CharBufferBuilder(_charBuffer);
            builder.TryAppendPartial("An error occurred during formatting: ");
            builder.TryAppendPartial(ex.Message);
            builder.TryAppendPartial(" - Unformatted message: ");

            var length = _message.WriteTo(builder.GetRemainingBuffer(), _config, LogMessage.FormatType.Unformatted, null);
            _charBufferLength = builder.Length + length;
        }
        catch
        {
            var builder = new CharBufferBuilder(_charBuffer);
            builder.TryAppendPartial("An error occurred during formatting: ");
            builder.TryAppendPartial(ex.Message);
            _charBufferLength = builder.Length;
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
}
