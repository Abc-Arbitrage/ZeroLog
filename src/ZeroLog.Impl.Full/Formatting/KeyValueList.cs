using System;
using System.Collections.Generic;

namespace ZeroLog.Formatting;

public sealed class KeyValueList
{
    private readonly List<InternalItem> _items = new(byte.MaxValue);

    private readonly char[] _buffer;
    private int _position;

    public int Count => _items.Count;

    internal KeyValueList(int bufferSize)
    {
        _buffer = new char[bufferSize];
    }

    public FormattedKeyValue this[int index]
    {
        get
        {
            var item = _items[index];
            return new FormattedKeyValue(item.Key, _buffer.AsSpan(item.ValueOffset, item.ValueLength), item.ValueType);
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

    public Enumerator GetEnumerator()
        => new(this);

    private readonly record struct InternalItem(
        string Key,
        int ValueOffset,
        int ValueLength,
        ArgumentType ValueType
    );

    public ref struct Enumerator
    {
        private readonly KeyValueList _keyValueList;
        private int _index;

        internal Enumerator(KeyValueList keyValueList)
        {
            _keyValueList = keyValueList;
            _index = -1;
        }

        public FormattedKeyValue Current => _keyValueList[_index];

        public bool MoveNext()
            => ++_index < _keyValueList.Count;
    }
}
