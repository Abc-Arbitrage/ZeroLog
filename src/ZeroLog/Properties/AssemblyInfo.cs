using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ZeroLog.Tests")]
[assembly: InternalsVisibleTo("ZeroLog.Benchmarks")]

#if NETCOREAPP && !NETCOREAPP2_1
[module: SkipLocalsInit]
#endif
