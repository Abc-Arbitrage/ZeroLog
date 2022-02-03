using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZeroLog;

internal class ObjectPool<T>
{
    private readonly ConcurrentQueue<T> _pool = new();
    private int _poolSize;

    public ObjectPool(int size, Func<T> factory)
    {
        _poolSize = size;

        for (var i = 0; i < size; i++)
            _pool.Enqueue(factory());
    }

    public bool TryAcquire([MaybeNullWhen(false)] out T instance)
        => _pool.TryDequeue(out instance);

    public void Release(T instance)
        => _pool.Enqueue(instance);

    public void Clear()
    {
        _pool.Clear();
        _poolSize = 0;
    }

    public bool IsAnyItemAcquired()
        => _pool.Count < _poolSize;
}
