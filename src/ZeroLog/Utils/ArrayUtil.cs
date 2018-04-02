using System;

namespace ZeroLog.Utils
{
    internal static class ArrayUtil
    {
        public static T[] Empty<T>()
        {
#if NET452
            return EmptyArray<T>.Instance;
#else
            return Array.Empty<T>();
#endif
        }

#if NET452
        private static class EmptyArray<T>
        {
            public static readonly T[] Instance = new T[0];
        }
#endif
    }
}
