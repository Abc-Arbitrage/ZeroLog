using System.Collections.Generic;
using System.Collections.Immutable;

namespace ZeroLog.Analyzers.Support;

internal class ImmutableArrayEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
{
    public static readonly ImmutableArrayEqualityComparer<T> Default = new();

    public bool Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if (x.Length != y.Length)
            return false;

        for (var i = 0; i < x.Length; ++i)
        {
            if (!EqualityComparer<T>.Default.Equals(x[i], y[i]))
                return false;
        }

        return true;
    }

    public int GetHashCode(ImmutableArray<T> obj)
    {
        unchecked
        {
            var hashCode = 0;

            foreach (var item in obj)
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(item);

            return hashCode;
        }
    }
}
