using System;

namespace ZeroLog
{
    internal class ObjectPool<T>
    {
        private readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();
        private int _poolSize;

        public ObjectPool(int size, Func<T> factory)
        {
            _poolSize = size;

            for (var i = 0; i < size; i++)
            {
                _pool.Enqueue(factory());
            }
        }

        public bool TryAcquire(out T instance)
            => _pool.TryDequeue(out instance);

        public void Release(T instance)
            => _pool.Enqueue(instance);

        public void Clear()
        {
            while (_pool.TryDequeue(out _))
            {
                --_poolSize;
            }
        }

        public bool IsAnyItemAcquired()
            => _pool.Count < _poolSize;
    }
}
