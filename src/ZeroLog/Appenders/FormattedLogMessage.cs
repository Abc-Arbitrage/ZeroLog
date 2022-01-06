using System;
using System.Runtime.CompilerServices;
using ZeroLog.Utils;

namespace ZeroLog.Appenders;

public class FormattedLogMessage
{
    private readonly char[] _charBuffer;

    private int _charLength;

    internal LogMessage Message = LogMessage.Empty; // TODO make private

    internal FormattedLogMessage(int bufferSize)
    {
        _charBuffer = GC.AllocateUninitializedArray<char>(bufferSize);

        SetMessage(LogMessage.Empty);
    }

    internal void SetMessage(LogMessage message)
    {
        Message = message;

        try
        {
            _charLength = Message.WriteTo(_charBuffer);
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

            var length = Message.WriteTo(builder.GetRemainingBuffer(), true);
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
