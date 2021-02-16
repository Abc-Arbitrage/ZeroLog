using System;
using BenchmarkDotNet.Attributes;
using ZeroLog.Utils;

namespace ZeroLog.Benchmarks.Utils
{
    public class HighResolutionDateTimeBenchmark
    {
        [Benchmark(Baseline = true)]
        public DateTime DateTime_UtcNow()
            => DateTime.UtcNow;

        [Benchmark]
        public DateTime HighResolutionDateTime_UtcNow()
            => HighResolutionDateTime.UtcNow;
    }
}
