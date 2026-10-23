using System;
using System.Collections.Generic;

namespace ZeroLog.Formatting;

/// <summary>
/// A list of log message metadata as key/value pairs.
/// </summary>
public sealed class KeyValueList
{
    private readonly List<InternalItem> _items;

    private readonly char[] _buffer;
    private readonly byte[] _rawDataBuffer;
    private int _position;
    private int _rawDataPosition;

    /// <summary>
    /// The number of items contained in the list.
    /// </summary>
    public int Count => _items.Count;

    internal KeyValueList(int bufferSize)
    {
        _items = new(byte.MaxValue);
        _buffer = GC.AllocateUninitializedArray<char>(bufferSize);
        _rawDataBuffer = GC.AllocateUninitializedArray<byte>(bufferSize);
    }

    internal KeyValueList(KeyValueList other)
    {
        _buffer = GC.AllocateUninitializedArray<char>(other._position);
        _rawDataBuffer = GC.AllocateUninitializedArray<byte>(other._rawDataPosition);

        other._buffer.AsSpan(0, other._position).CopyTo(_buffer);
        other._rawDataBuffer.AsSpan(0, other._rawDataPosition).CopyTo(_rawDataBuffer);

        _position = other._position;
        _rawDataPosition = other._rawDataPosition;

        _items = new(other._items);
    }

    /// <summary>
    /// Gets the item at the specified index.
    /// </summary>
    /// <param name="index">The item index.</param>
    public LoggedKeyValue this[int index]
    {
        get
        {
            var item = _items[index];

            return new LoggedKeyValue(
                item.Key,
                item.ArgumentType,
                _buffer.AsSpan(item.StringValueOffset, item.StringValueLength),
                _rawDataBuffer.AsSpan(item.RawDataOffset, item.RawDataLength)
            );
        }
    }

    internal void Clear()
    {
        _position = 0;
        _rawDataPosition = 0;
        _items.Clear();
    }

    internal Span<char> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    internal bool Add(string key, string? value)
    {
        if (value is null)
        {
            _items.Add(new InternalItem(key, ArgumentType.Null, 0, 0, 0, 0));
            return true;
        }

        var outputBuffer = GetRemainingBuffer();
        var charsToCopy = Math.Min(value.Length, outputBuffer.Length);
        value.AsSpan(0, charsToCopy).CopyTo(outputBuffer);

        _items.Add(new InternalItem(key, ArgumentType.String, _position, charsToCopy, 0, 0));
        _position += charsToCopy;

        return charsToCopy == value.Length;
    }

    internal bool Add(string key, ArgumentType argumentType, int stringValueLength, ReadOnlySpan<byte> rawData)
    {
        if (_rawDataPosition + rawData.Length > _rawDataBuffer.Length)
            return false;

        rawData.CopyTo(_rawDataBuffer.AsSpan(_rawDataPosition));
        _items.Add(new InternalItem(key, argumentType, _position, stringValueLength, _rawDataPosition, rawData.Length));

        _position += stringValueLength;
        _rawDataPosition += rawData.Length;

        return true;
    }

    /// <summary>
    /// Gets an enumerator over this list.
    /// </summary>
    public Enumerator GetEnumerator()
        => new(this);

    private record struct InternalItem(
        string Key,
        ArgumentType ArgumentType,
        int StringValueOffset,
        int StringValueLength,
        int RawDataOffset,
        int RawDataLength
    );

    /// <summary>
    /// An enumerator over a <see cref="KeyValueList"/>.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly KeyValueList _keyValueList;
        private int _index;

        internal Enumerator(KeyValueList keyValueList)
        {
            _keyValueList = keyValueList;
            _index = -1;
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        public LoggedKeyValue Current => _keyValueList[_index];

        /// <summary>
        /// Moves to the next item.
        /// </summary>
        public bool MoveNext()
            => ++_index < _keyValueList.Count;
    }
}
