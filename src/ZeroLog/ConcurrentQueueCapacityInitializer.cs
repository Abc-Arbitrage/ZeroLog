using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZeroLog
{
    /// <summary>
    /// Fake collection used to initialize the capacity of a ConcurrentQueue :
    /// - Has a Count property set to the desired initial capacity
    /// - Has a noop iterator
    /// </summary>
    internal class ConcurrentQueueCapacityInitializer : ICollection<LogEvent>
    {
        public int Count { get; }
        public bool IsReadOnly => true;

        public ConcurrentQueueCapacityInitializer(int size)
        {
            Count = size;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<LogEvent> GetEnumerator() => Enumerable.Empty<LogEvent>().GetEnumerator();

        public void Add(LogEvent item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(LogEvent item) => throw new NotSupportedException();
        public void CopyTo(LogEvent[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(LogEvent item) => throw new NotSupportedException();
    }
}
