using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace ZeroLog.Formatting;

[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
internal ref struct CharBufferBuilder
{
    private readonly Span<char> _buffer;
    private int _pos;

    public int Length => _pos;

    public CharBufferBuilder(Span<char> buffer)
    {
        _buffer = buffer;
        _pos = 0;
    }

    public ReadOnlySpan<char> GetOutput()
        => _buffer.Slice(0, _pos);

    public Span<char> GetRemainingBuffer()
        => _buffer.Slice(_pos);

    public void IncrementPos(int chars)
        => _pos += chars;

    /// <summary>
    /// Appends a character, but does nothing if there is no more room for it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char value)
    {
        if (_pos < _buffer.Length)
            _buffer[_pos++] = value;
    }

    public bool TryAppendWhole(ReadOnlySpan<char> value)
    {
        if (value.Length <= _buffer.Length - _pos)
        {
            value.CopyTo(_buffer.Slice(_pos));
            _pos += value.Length;
            return true;
        }

        return false;
    }

    public bool TryAppendPartial(ReadOnlySpan<char> value)
    {
        if (value.Length <= _buffer.Length - _pos)
        {
            value.CopyTo(_buffer.Slice(_pos));
            _pos += value.Length;
            return true;
        }

        var length = _buffer.Length - _pos;
        value.Slice(0, length).CopyTo(_buffer.Slice(_pos));
        _pos += length;
        return false;
    }

    public void TryAppendPartial(char value, int count)
    {
        if (count > 0)
        {
            count = Math.Min(count, _buffer.Length - _pos);
            _buffer.Slice(_pos, count).Fill(value);
            _pos += count;
        }
    }

    public bool TryAppend(int value, string? format = null)
    {
        if (!value.TryFormat(_buffer.Slice(_pos), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        _pos += charsWritten;
        return true;
    }

    public bool TryAppend(char value)
    {
        if (_pos < _buffer.Length)
        {
            _buffer[_pos] = value;
            ++_pos;
            return true;
        }

        return false;
    }

    public bool TryAppend(DateTime value, string? format)
    {
        if (!value.TryFormat(_buffer.Slice(_pos), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        _pos += charsWritten;
        return true;
    }

    public bool TryAppend(TimeSpan value, string? format)
    {
        if (!value.TryFormat(_buffer.Slice(_pos), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        _pos += charsWritten;
        return true;
    }

    public bool TryAppend<T>(T value, string? format = null)
        where T : struct, ISpanFormattable
    {
        if (!value.TryFormat(_buffer.Slice(_pos), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        _pos += charsWritten;
        return true;
    }

    public override string ToString()
        => GetOutput().ToString();
}
