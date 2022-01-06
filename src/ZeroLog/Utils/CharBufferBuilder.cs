using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ZeroLog.Utils;

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
}
