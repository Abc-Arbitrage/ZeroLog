using System;
using System.Text;

namespace ZeroLog.Formatting;

/// <summary>
/// A formatter which converts a logged message to UTF-8 encoded text.
/// </summary>
public abstract class Utf8Formatter
{
    /// <summary>
    /// This is equal to <c>UTF8Encoding.MaxUtf8BytesPerChar</c>.
    /// </summary>
    /// <remarks>
    /// Code points encoded as 4 bytes in UTF-8 are represented by a surrogate pair in UTF-16.
    /// </remarks>
    internal const int MaxUtf8BytesPerChar = 3;

    private static readonly byte[] _newLineBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

    private readonly byte[] _buffer = GC.AllocateUninitializedArray<byte>(LogManager.OutputBufferSize * MaxUtf8BytesPerChar);
    private int _position;

    /// <summary>
    /// Formats the given message to text.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>A span representing the text to log.</returns>
    public ReadOnlySpan<byte> FormatMessage(LoggedMessage message)
    {
        _position = 0;
        WriteMessage(message);
        return GetOutput();
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
    protected internal void Write(ReadOnlySpan<byte> value)
    {
        var charCount = Math.Min(value.Length, _buffer.Length - _position);
        value.Slice(0, charCount).CopyTo(_buffer.AsSpan(_position));
        _position += charCount;
    }

    /// <summary>
    /// Appends text followed by a newline to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void WriteLine(ReadOnlySpan<byte> value)
    {
        Write(value);
        WriteLine();
    }

    /// <summary>
    /// Appends a newline to the output.
    /// </summary>
    /// <remarks>
    /// If the buffer is full, the newline will be inserted by overwriting the last characters.
    /// </remarks>
    protected internal void WriteLine()
    {
        if (_position <= _buffer.Length - _newLineBytes.Length)
        {
            _newLineBytes.AsSpan().CopyTo(_buffer.AsSpan(_position));
            _position += _newLineBytes.Length;
        }
        else
        {
            // Make sure to end the string with a newline
            _newLineBytes.AsSpan().CopyTo(_buffer.AsSpan(_buffer.Length - _newLineBytes.Length));
        }
    }

    /// <summary>
    /// Returns a span of the current output.
    /// </summary>
    protected internal Span<byte> GetOutput()
        => _buffer.AsSpan(0, _position);

    /// <summary>
    /// Returns a span of the remaining buffer. Call <see cref="AdvanceBy"/> after modifying it.
    /// </summary>
    protected Span<byte> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    /// <summary>
    /// Advances the position on the buffer returned by <see cref="GetRemainingBuffer"/> by <paramref name="charCount"/>.
    /// </summary>
    /// <param name="charCount">The character count to advance the position by.</param>
    protected void AdvanceBy(int charCount)
        => _position += charCount;

    internal byte[] GetBuffer(out int length)
    {
        length = _position;
        return _buffer;
    }
}
