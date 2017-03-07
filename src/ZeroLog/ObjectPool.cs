using System;

namespace ZeroLog
{
    internal class ObjectPool<T>
    {
        private readonly ConcurrentQueue<T> _pool = new ConcurrentQueue<T>();

        public ObjectPool(int size, Func<T> factory)
        {
            for (var i = 0; i < size; i++)
            {
                _pool.Enqueue(factory());
            }
        }

        public bool TryAcquire(out T instance)
        {
            return _pool.TryDequeue(out instance);
        }

        public void Release(T instance)
        {
            _pool.Enqueue(instance);
        }
    }
}
