using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZeroLog.Support;

internal class ConcurrentQueueCapacityInitializer<T> : ICollection<T>
{
    // Fake collection used to initialize the capacity of a ConcurrentQueue:
    // - Has a Count property set to the desired initial capacity
    // - Has a noop iterator

    public int Count { get; }
    public bool IsReadOnly => true;

    public ConcurrentQueueCapacityInitializer(int size)
    {
        Count = size;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<T> GetEnumerator() => Enumerable.Empty<T>().GetEnumerator();

    public void Add(T item) => throw new NotSupportedException();
    public void Clear() => throw new NotSupportedException();
    public bool Contains(T item) => throw new NotSupportedException();
    public void CopyTo(T[] array, int arrayIndex) => throw new NotSupportedException();
    public bool Remove(T item) => throw new NotSupportedException();
}
