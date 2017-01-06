using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ZeroLog
{
    internal class FakeCollection : ICollection<LogEvent>
    {
        private readonly int _size;

        public FakeCollection(int size)
        {
            _size = size;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<LogEvent> GetEnumerator()
        {
            return Enumerable.Empty<LogEvent>().GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count => _size;
        public object SyncRoot { get; }
        public bool IsSynchronized { get; }

        public void Add(LogEvent item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(LogEvent item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(LogEvent[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(LogEvent item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly { get; }
    }
}