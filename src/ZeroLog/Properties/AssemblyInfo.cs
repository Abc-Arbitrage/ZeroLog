using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZeroLog.Tests")]
[assembly: InternalsVisibleTo("ZeroLog.Benchmarks")]

#if NETCOREAPP
[module: SkipLocalsInit]
#endif
