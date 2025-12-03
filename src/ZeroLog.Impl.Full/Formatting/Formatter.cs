using System;
using System.Globalization;

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
        return GetOutput();
    }

    /// <summary>
    /// Formats the given message to text.
    /// </summary>
    /// <remarks>
    /// Call <see cref="Write(System.ReadOnlySpan{char})"/> to append text to the output.
    /// </remarks>
    /// <param name="message">The message to format.</param>
    protected abstract void WriteMessage(LoggedMessage message);

    /// <summary>
    /// Appends text to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void Write(ReadOnlySpan<char> value)
    {
        var charCount = Math.Min(value.Length, _buffer.Length - _position);
        value.Slice(0, charCount).CopyTo(_buffer.AsSpan(_position));
        _position += charCount;
    }

    /// <summary>
    /// Appends a <see cref="ISpanFormattable"/> to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    /// <param name="format">The format to use.</param>
    protected internal bool Write<T>(T value, ReadOnlySpan<char> format = default)
        where T : ISpanFormattable
    {
        if (!value.TryFormat(GetRemainingBuffer(), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        AdvanceBy(charsWritten);
        return true;
    }

    /// <summary>
    /// Appends a <see cref="KeyValueList"/> to the output as JSON.
    /// </summary>
    /// <param name="keyValues">The list to write.</param>
    protected internal void Write(KeyValueList? keyValues)
    {
        if (keyValues is null)
            return;

        JsonWriter.WriteJsonToStringBuffer(keyValues, GetRemainingBuffer(), out var charsWritten);
        AdvanceBy(charsWritten);
    }

    /// <summary>
    /// Appends text followed by a newline to the output.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected internal void WriteLine(ReadOnlySpan<char> value)
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
        if (_position <= _buffer.Length - Environment.NewLine.Length)
        {
            Environment.NewLine.AsSpan().CopyTo(_buffer.AsSpan(_position));
            _position += Environment.NewLine.Length;
        }
        else
        {
            // Make sure to end the string with a newline
            Environment.NewLine.AsSpan().CopyTo(_buffer.AsSpan(_buffer.Length - Environment.NewLine.Length));
        }
    }

    /// <summary>
    /// Pads the output with spaces until the specified column is reached.
    /// </summary>
    /// <param name="column">The column number.</param>
    protected internal void PadToColumn(int column)
    {
        if (_position >= column)
            return;

        var length = Math.Min(column, _buffer.Length) - _position;
        _buffer.AsSpan(_position, length).Fill(' ');
        _position += length;
    }

    /// <summary>
    /// Returns a span of the current output.
    /// </summary>
    protected internal Span<char> GetOutput()
        => _buffer.AsSpan(0, _position);

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
        => _position = Math.Min(_position + charCount, _buffer.Length);

    internal char[] GetBuffer(out int length)
    {
        length = _position;
        return _buffer;
    }
}
