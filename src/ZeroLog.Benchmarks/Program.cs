using System;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ZeroLog.Benchmarks.EnumTests;
using ZeroLog.Benchmarks.LatencyTests;
using ZeroLog.Benchmarks.ThroughputTests;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks
{
    public class Program
    {
        private static void Throughput()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(StatisticColumn.P90);
            config.Add(StatisticColumn.P95);

            BenchmarkRunInfo benchs = BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputBenchmarks));

            BenchmarkRunner.Run(benchs.Benchmarks, config);
        }

        private static void LatencyMultiProducer(int threadCount, int warmupMessageCount, int messageCount, int queueSize)
        {
            var zeroLog = new ZeroLogMultiProducer().Bench(queueSize, warmupMessageCount, messageCount, threadCount);
            var nlogSync = new NLogSyncMultiProducer().Bench(warmupMessageCount, messageCount, threadCount);
            var nlogAsync = new NLogAsyncMultiProducer().Bench(queueSize, warmupMessageCount, messageCount, threadCount);
            var log4net = new Log4NetMultiProducer().Bench(warmupMessageCount, messageCount, threadCount);

            SimpleLatencyBenchmark.PrintSummary($"{threadCount} producers, {messageCount:N0} total log events (queue size={queueSize:N0}) - unit is *us*",
                                                ("ZeroLog", zeroLog),
                                                ("NLogSync", nlogSync),
                                                ("NLogAsync", nlogAsync),
                                                ("Log4net", log4net));
        }

        public static void Main()
        {
            //Throughput();

            //LatencyMultiProducer(4, 4 * 25_000, 4 * 250_000, 64);
            //LatencyMultiProducer(8, 8 * 25_000, 8 * 250_000, 64);
            //LatencyMultiProducer(4, 4 * 25_000, 4 * 250_000, 1024);
            //LatencyMultiProducer(8, 8 * 25_000, 8 * 250_000, 1024);

            EnumBenchmarksRunner.Run();

            Console.ReadLine();
        }
    }
}
