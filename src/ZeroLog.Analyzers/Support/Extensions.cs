using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

    extension<T>(ImmutableArray<T> array)
        where T : struct, IEquatable<T>
    {
        public bool HasEqualValues(ImmutableArray<T> other)
            => array.AsSpan().SequenceEqual(other.AsSpan());

        public int GetValuesHashCode()
        {
            if (array.IsDefault)
                return 0;

            unchecked
            {
                var hashCode = 0;

                foreach (var item in array)
                    hashCode = (hashCode * 397) ^ item.GetHashCode();

                return hashCode;
            }
        }
    }
}
