using System;

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
        _charLength = -1;
    }

    public ReadOnlySpan<char> GetMessage()
    {
        if (_charLength < 0)
        {
            _charLength = Message.WriteTo(_charBuffer);
        }

        return _charBuffer.AsSpan(0, _charLength);
    }

    public override string ToString()
        => GetMessage().ToString();
}
