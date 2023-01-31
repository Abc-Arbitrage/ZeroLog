using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ZeroLog.Support;

namespace ZeroLog;

internal class ObjectPool<T> : IDisposable
{
    private readonly ConcurrentQueue<T> _pool;
    private readonly Func<T> _factory;
    private int _capacity;
    private int _count; // _pool.Count can be expensive

    public int Count => _count;

    public ObjectPool(int size, Func<T> factory)
    {
        _capacity = size;
        _count = size;
        _factory = factory;

        _pool = new ConcurrentQueue<T>(new ConcurrentQueueCapacityInitializer<T>(size));

        for (var i = 0; i < size; i++)
            _pool.Enqueue(CreateObject());
    }

    public bool TryAcquire([MaybeNullWhen(false)] out T instance)
    {
        if (_pool.TryDequeue(out instance))
        {
            Interlocked.Decrement(ref _count);
            return true;
        }

        return false;
    }

    public void Release(T instance)
    {
        // Possible benign race condition: we may enqueue more items than the pool capacity

        if (_count < _capacity)
        {
            _pool.Enqueue(instance);
            Interlocked.Increment(ref _count);
        }
    }

    public void Dispose()
    {
        _pool.Clear();
        _capacity = 0;
        _count = 0;
    }

    public T CreateObject()
        => _factory();
}
