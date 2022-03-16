using System;

namespace ZeroLog.Formatting;

/// <summary>
/// A formatter which converts a logged message to text.
/// </summary>
public abstract class Formatter
{
    private readonly char[] _buffer = GC.AllocateUninitializedArray<char>(LogManager.OutputBufferSize);
    private int _position;

    /// <summary>
    /// Formats the given message to text.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>A span representing the text to log.</returns>
    public ReadOnlySpan<char> FormatMessage(LoggedMessage message)
    {
        _position = 0;
        WriteMessage(message);
        return _buffer.AsSpan(0, _position);
    }

    /// <summary>
    /// Formats the given message to text.
    /// </summary>
    /// <remarks>
    /// Call <see cref="Write"/> to append text to the output.
    /// </remarks>
    /// <param name="message">The message to format.</param>
    protected abstract void WriteMessage(LoggedMessage message);

    /// <summary>
    /// Appends text to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected void Write(ReadOnlySpan<char> value)
    {
        var charCount = Math.Min(value.Length, _buffer.Length - _position);
        value.Slice(0, charCount).CopyTo(_buffer.AsSpan(_position));
        _position += charCount;
    }

    /// <summary>
    /// Appends text followed by a newline to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected void WriteLine(ReadOnlySpan<char> value)
    {
        Write(value);
        Write(Environment.NewLine);
    }

    /// <summary>
    /// Appends a newline to the output.
    /// </summary>
    protected void WriteLine()
        => Write(Environment.NewLine);

    /// <summary>
    /// Returns a span of the remaining buffer. Call <see cref="AdvanceBy"/> after modifying it.
    /// </summary>
    protected Span<char> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    /// <summary>
    /// Advances the position on the buffer returned by <see cref="GetRemainingBuffer"/> by <paramref name="charCount"/>.
    /// </summary>
    /// <param name="charCount">The character count to advance the position by.</param>
    protected void AdvanceBy(int charCount)
        => _position += charCount;
}
