using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ZeroLog.Utils;

namespace ZeroLog.Appenders;

public class FormattedLogMessage
{
    private readonly char[] _charBuffer;
    private readonly KeyValuePointerBuffer _keyValuePointerBuffer = new();
    private int _charLength;

    private LogMessage _message = LogMessage.Empty;

    public Level Level => _message.Level;
    public DateTime Timestamp => _message.Timestamp;
    public Thread? Thread => _message.Thread;
    public Exception? Exception => _message.Exception;
    public string? LoggerName => _message.Logger?.Name;

    internal FormattedLogMessage(int bufferSize)
    {
        _charBuffer = GC.AllocateUninitializedArray<char>(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    internal void SetMessage(LogMessage message)
    {
        _message = message;

        try
        {
            _charLength = _message.WriteTo(_charBuffer, false, _keyValuePointerBuffer);

            if (_keyValuePointerBuffer.KeyPointerCount != 0)
                AppendKeyValues();
        }
        catch (Exception ex)
        {
            HandleFormattingError(ex);
        }
    }

    private void AppendKeyValues()
    {
        var builder = new CharBufferBuilder(_charBuffer.AsSpan(_charLength));
        if (!builder.TryAppendWhole(LogManager.Config.JsonSeparator))
            return;

        JsonWriter.WriteJsonToStringBuffer(ref builder, _keyValuePointerBuffer);
        _charLength += builder.Length;
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

            var length = _message.WriteTo(builder.GetRemainingBuffer(), true);
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