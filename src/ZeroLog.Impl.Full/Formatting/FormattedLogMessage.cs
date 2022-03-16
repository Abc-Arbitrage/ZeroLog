using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Configuration;

namespace ZeroLog.Formatting;

public sealed class FormattedLogMessage
{
    private readonly ZeroLogConfiguration _config;
    private readonly char[] _charBuffer;
    private int _charLength;

    private LogMessage _message = LogMessage.Empty;

    public LogLevel Level => _message.Level;
    public DateTime Timestamp => _message.Timestamp;
    public Thread? Thread => _message.Thread;
    public Exception? Exception => _message.Exception;
    public string? LoggerName => _message.Logger?.Name;

    public KeyValueList KeyValues { get; }

    internal FormattedLogMessage(int bufferSize, ZeroLogConfiguration config)
    {
        _config = config;
        _charBuffer = GC.AllocateUninitializedArray<char>(bufferSize);
        KeyValues = new(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    internal void SetMessage(LogMessage message)
    {
        _message = message;

        try
        {
            _charLength = _message.WriteTo(_charBuffer, _config, LogMessage.FormatType.Formatted, KeyValues);
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
            var builder = new CharBufferBuilder(_charBuffer);
            builder.TryAppendPartial("An error occured during formatting: ");
            builder.TryAppendPartial(ex.Message);
            builder.TryAppendPartial(" - Unformatted message: ");

            var length = _message.WriteTo(builder.GetRemainingBuffer(), _config, LogMessage.FormatType.Unformatted);
            _charLength = builder.Length + length;
        }
        catch
        {
            var builder = new CharBufferBuilder(_charBuffer);
            builder.TryAppendPartial("An error occured during formatting: ");
            builder.TryAppendPartial(ex.Message);
            _charLength = builder.Length;
        }
    }

    public ReadOnlySpan<char> GetMessage()
        => _charBuffer.AsSpan(0, _charLength);

    public override string ToString()
        => GetMessage().ToString();
}
