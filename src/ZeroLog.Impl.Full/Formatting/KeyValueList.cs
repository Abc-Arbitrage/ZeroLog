using System;
using System.Collections.Generic;

namespace ZeroLog.Formatting;

/// <summary>
/// A list of log message metadata as key/value pairs.
/// </summary>
public sealed class KeyValueList
{
    private readonly List<InternalItem> _items = new(byte.MaxValue);

    private readonly char[] _buffer;
    private int _position;

    /// <summary>
    /// The number of items contained in the list.
    /// </summary>
    public int Count => _items.Count;

    internal KeyValueList(int bufferSize)
    {
        _buffer = new char[bufferSize];
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
            return new LoggedKeyValue(item.Key, _buffer.AsSpan(item.ValueOffset, item.ValueLength), item.ValueType);
        }
    }

    internal void Clear()
    {
        _position = 0;
        _items.Clear();
    }

    internal Span<char> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    internal void Add(string key, int valueLength, ArgumentType valueType)
    {
        _items.Add(new(key, _position, valueLength, valueType));
        _position += valueLength;
    }

    /// <summary>
    /// Gets an enumerator over this list.
    /// </summary>
    public Enumerator GetEnumerator()
        => new(this);

    private readonly record struct InternalItem(
        string Key,
        int ValueOffset,
        int ValueLength,
        ArgumentType ValueType
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
