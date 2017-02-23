using System;
using System.Collections.Concurrent;

namespace ZeroLog
{
    internal class ObjectPool<T>
    {
        private readonly ConcurrentStack<T> _pool = new ConcurrentStack<T>();

        public ObjectPool(int size, Func<T> factory)
        {
            for (var i = 0; i < size; i++)
            {
                _pool.Push(factory());
            }
        }

        public bool TryAcquire(out T instance)
        {
            return _pool.TryPop(out instance);
        }

        public void Release(T instance)
        {
            _pool.Push(instance);
        }
    }
}
