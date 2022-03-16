using System;

namespace ZeroLog.Formatting;

public abstract class Formatter
{
    private readonly char[] _buffer = GC.AllocateUninitializedArray<char>(LogManager.OutputBufferSize);
    private int _position;

    public ReadOnlySpan<char> FormatMessage(FormattedLogMessage message)
    {
        _position = 0;
        WriteMessage(message);
        return _buffer.AsSpan(0, _position);
    }

    protected abstract void WriteMessage(FormattedLogMessage message);

    protected void Write(ReadOnlySpan<char> value)
    {
        var charCount = Math.Min(value.Length, _buffer.Length - _position);
        value.Slice(0, charCount).CopyTo(_buffer.AsSpan(_position));
        _position += charCount;
    }

    protected void WriteLine(ReadOnlySpan<char> value)
    {
        Write(value);
        Write(Environment.NewLine);
    }

    protected void WriteLine()
        => Write(Environment.NewLine);

    protected Span<char> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    protected void AdvanceBy(int charCount)
        => _position += charCount;
}
