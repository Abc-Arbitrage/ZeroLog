using System.Linq;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ZeroLog.Benchmarks.Latency;

namespace ZeroLog.Benchmarks
{
    public class Program
    {
        public static void Main()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(StatisticColumn.P90);
            config.Add(StatisticColumn.P95);

            var benchs = BenchmarkConverter.TypeToBenchmarks(typeof(ZeroLogSingleProducer))
                            .Union(BenchmarkConverter.TypeToBenchmarks(typeof(Log4NetSingleProducer)))
                            .Union(BenchmarkConverter.TypeToBenchmarks(typeof(NLogSyncSingleProducer)))
                            .Union(BenchmarkConverter.TypeToBenchmarks(typeof(NLogAsyncSingleProducer)))
                            .ToArray();

            BenchmarkRunner.Run(benchs, config);
        }
    }
}
