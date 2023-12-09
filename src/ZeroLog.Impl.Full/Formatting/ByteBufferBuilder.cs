#if NET8_0_OR_GREATER

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;

namespace ZeroLog.Formatting;

[SuppressMessage("ReSharper", "ReplaceSliceWithRangeIndexer")]
internal ref struct ByteBufferBuilder
{
    private readonly Span<byte> _buffer;
    private int _pos;

    public int Length => _pos;

    public ByteBufferBuilder(Span<byte> buffer)
    {
        _buffer = buffer;
        _pos = 0;
    }

    public ReadOnlySpan<byte> GetOutput()
        => _buffer.Slice(0, _pos);

    public Span<byte> GetRemainingBuffer()
        => _buffer.Slice(_pos);

    public void IncrementPos(int chars)
        => _pos += chars;

    /// <summary>
    /// Appends a character, but does nothing if there is no more room for it.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendAscii(char value)
    {
        if (_pos < _buffer.Length)
            _buffer[_pos++] = (byte)value;
    }

    public bool TryAppendWhole(ReadOnlySpan<byte> value)
    {
        if (value.Length <= _buffer.Length - _pos)
        {
            value.CopyTo(_buffer.Slice(_pos));
            _pos += value.Length;
            return true;
        }

        return false;
    }

    public bool TryAppendWhole(ReadOnlySpan<char> value)
    {
        if (Utf8.FromUtf16(value, _buffer.Slice(_pos), out _, out var bytesWritten) == OperationStatus.Done)
        {
            _pos += bytesWritten;
            return true;
        }

        return false;
    }

    public bool TryAppendPartial(ReadOnlySpan<byte> value)
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

    public bool TryAppendPartialChars(scoped ReadOnlySpan<char> value)
    {
        var status = Utf8.FromUtf16(value, _buffer.Slice(_pos), out _, out var bytesWritten);
        _pos += bytesWritten;
        return status == OperationStatus.Done;
    }

    public void TryAppendPartialAscii(char value, int count)
    {
        if (count > 0)
        {
            count = Math.Min(count, _buffer.Length - _pos);
            _buffer.Slice(_pos, count).Fill((byte)value);
            _pos += count;
        }
    }

    public bool TryAppendAscii(char value)
    {
        if (_pos < _buffer.Length)
        {
            _buffer[_pos] = (byte)value;
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
        where T : struct, IUtf8SpanFormattable
    {
        if (!value.TryFormat(_buffer.Slice(_pos), out var charsWritten, format, CultureInfo.InvariantCulture))
            return false;

        _pos += charsWritten;
        return true;
    }

    public override string ToString()
        => Encoding.UTF8.GetString(GetOutput());
}

#endif
