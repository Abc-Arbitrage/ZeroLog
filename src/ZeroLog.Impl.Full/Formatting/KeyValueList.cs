using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ZeroLog.Formatting;

/// <summary>
/// A list of log message metadata as key/value pairs.
/// </summary>
public sealed unsafe class KeyValueList
{
    private readonly List<InternalItem> _items;

    private readonly char[] _buffer;
    private readonly byte[]? _rawDataBuffer;
    private int _position;

    /// <summary>
    /// The number of items contained in the list.
    /// </summary>
    public int Count => _items.Count;

    internal KeyValueList(int bufferSize)
    {
        _items = new(byte.MaxValue);
        _buffer = GC.AllocateUninitializedArray<char>(bufferSize);
    }

    internal KeyValueList(KeyValueList other)
    {
        _buffer = GC.AllocateUninitializedArray<char>(other._position);
        other._buffer.AsSpan(0, other._position).CopyTo(_buffer);
        _position = other._position;
        _items = new(other._items);

        if (other._rawDataBuffer is null)
        {
            _rawDataBuffer = GC.AllocateUninitializedArray<byte>(_items.Sum(item => item.RawDataLength));
            var offset = 0;

            foreach (ref var item in CollectionsMarshal.AsSpan(_items))
            {
                new Span<byte>((void*)item.RawDataPointerOrOffset, item.RawDataLength).CopyTo(_rawDataBuffer.AsSpan(offset, item.RawDataLength));
                item.RawDataPointerOrOffset = (nuint)offset;
                offset += item.RawDataLength;
            }
        }
        else
        {
            _rawDataBuffer = other._rawDataBuffer;
        }
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
                _buffer.AsSpan(item.StringValueOffset, item.StringValueLength),
                _rawDataBuffer is null
                    ? new ReadOnlySpan<byte>((void*)item.RawDataPointerOrOffset, item.RawDataLength)
                    : _rawDataBuffer.AsSpan((int)item.RawDataPointerOrOffset, item.RawDataLength)
            );
        }
    }

    internal void Clear()
    {
        _position = 0;
        _items.Clear();
    }

    internal Span<char> GetRemainingBuffer()
        => _buffer.AsSpan(_position);

    internal void Add(string key, int stringValueLength, byte* rawDataPointer, int rawDataLength)
    {
        _items.Add(new InternalItem(key, _position, stringValueLength, (nuint)rawDataPointer, rawDataLength));
        _position += stringValueLength;
    }

    /// <summary>
    /// Gets an enumerator over this list.
    /// </summary>
    public Enumerator GetEnumerator()
        => new(this);

    private record struct InternalItem(
        string Key,
        int StringValueOffset,
        int StringValueLength,
        nuint RawDataPointerOrOffset,
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
