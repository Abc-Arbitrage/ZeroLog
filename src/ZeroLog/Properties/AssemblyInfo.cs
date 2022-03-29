using System.Runtime.CompilerServices;
using ZeroLog;

[assembly: InternalsVisibleTo($"ZeroLog.Tests, PublicKey={AssemblyData.PublicKey}")]
[assembly: InternalsVisibleTo($"ZeroLog.Tests.NetStandard, PublicKey={AssemblyData.PublicKey}")]
[assembly: InternalsVisibleTo($"ZeroLog.Benchmarks, PublicKey={AssemblyData.PublicKey}")]

#if NETCOREAPP
[module: SkipLocalsInit]
#endif
