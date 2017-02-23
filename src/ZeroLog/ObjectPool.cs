// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

// define TRACE_LEAKS to get additional diagnostics that can lead to the leak sources. note: it will
// make everything about 2-3x slower
// 
// #define TRACE_LEAKS

// define DETECT_LEAKS to detect possible leaks
// #if DEBUG
// #define DETECT_LEAKS  //for now always enable DETECT_LEAKS in debug.
// #endif

using System;
using System.Collections.Concurrent;

namespace ZeroLog
{
    public class ObjectPool<T>
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
