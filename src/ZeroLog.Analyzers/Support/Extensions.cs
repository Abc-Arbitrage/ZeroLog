using System.Collections.Generic;

namespace ZeroLog.Analyzers.Support;

internal static class Extensions
{
    extension<T>(IEnumerable<T> source)
    {
        public T? UniqueOrDefault()
        {
            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
                return default;

            var first = enumerator.Current;

            if (enumerator.MoveNext())
                return default;

            return first;
        }
    }
}
