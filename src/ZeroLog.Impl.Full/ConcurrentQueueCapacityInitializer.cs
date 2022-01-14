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
    internal class ConcurrentQueueCapacityInitializer : ICollection<LogMessage>
    {
        public int Count { get; }
        public bool IsReadOnly => true;

        public ConcurrentQueueCapacityInitializer(int size)
        {
            Count = size;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<LogMessage> GetEnumerator() => Enumerable.Empty<LogMessage>().GetEnumerator();

        public void Add(LogMessage item) => throw new NotSupportedException();
        public void Clear() => throw new NotSupportedException();
        public bool Contains(LogMessage item) => throw new NotSupportedException();
        public void CopyTo(LogMessage[] array, int arrayIndex) => throw new NotSupportedException();
        public bool Remove(LogMessage item) => throw new NotSupportedException();
    }
}
