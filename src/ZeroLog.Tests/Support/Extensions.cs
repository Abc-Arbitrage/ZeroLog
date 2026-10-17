using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZeroLog.Tests.Support;

#nullable enable

internal static class Extensions
{
    public static string? GetMetadataValue(this Assembly assembly, string key)
        => assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                   .FirstOrDefault(attr => attr.Key == key)?
                   .Value;

    public static string Join(this IEnumerable<string> values, string separator)
        => string.Join(separator, values);
}
