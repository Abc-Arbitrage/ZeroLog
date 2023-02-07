using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using ZeroLog.Support;

#pragma warning disable CS0169

namespace ZeroLog;

internal class ObjectPool<T> : IDisposable
    where T : class
{
    private readonly ConcurrentQueue<T> _queue;
    private readonly Func<T> _factory;

    private object? _paddingBefore16;
    private object? _paddingBefore24;
    private object? _paddingBefore32;
    private object? _paddingBefore40;
    private object? _paddingBefore48;
    private object? _paddingBefore56;

    private T? _cached; // Put this on its own cache line

    private object? _paddingAfter8;
    private object? _paddingAfter16;
    private object? _paddingAfter24;
    private object? _paddingAfter32;
    private object? _paddingAfter40;
    private object? _paddingAfter48;
    private object? _paddingAfter56;

    private int _queueCapacity;
    private int _queueCount; // _pool.Count can be expensive

    public int Count => _queueCount + (_cached is not null ? 1 : 0);

    public ObjectPool(int size, Func<T> factory)
    {
        _queueCapacity = size;
        _queueCount = _queueCapacity;
        _factory = factory;

        _queue = new ConcurrentQueue<T>(new ConcurrentQueueCapacityInitializer<T>(_queueCapacity));

        for (var i = 0; i < _queueCapacity; ++i)
            _queue.Enqueue(CreateObject());
    }

    public bool TryAcquire([MaybeNullWhen(false)] out T instance)
    {
        var cached = Interlocked.Exchange(ref _cached, null);
        if (cached is not null)
        {
            instance = cached;
            return true;
        }

        if (_queue.TryDequeue(out instance))
        {
            Interlocked.Decrement(ref _queueCount);
            return true;
        }

        return false;
    }

    public void Release(T instance)
    {
        var cached = Interlocked.Exchange(ref _cached, instance);
        if (cached is null)
            return;

        // We need to check for the capacity, as more objects can be released than were acquired
        // when the "allocate" pool exhaustion strategy is used.
        // There is a race condition between the Enqueue and Increment calls,
        // so we may still exceed the capacity, but this is benign.

        if (_queueCount < _queueCapacity)
        {
            _queue.Enqueue(cached);
            Interlocked.Increment(ref _queueCount);
        }
    }

    public void Dispose()
    {
        _queue.Clear();
        _queueCapacity = 0;
        _queueCount = 0;
        _cached = null;
    }

    public T CreateObject()
        => _factory();
}
