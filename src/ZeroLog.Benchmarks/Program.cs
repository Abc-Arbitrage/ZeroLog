using System.Linq;
﻿using System;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ZeroLog.Benchmarks.HandmadeTest;
using ZeroLog.Benchmarks.Latency;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks
{
    public class Program
    {
        private static void LatencySingleProducer()
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

        private static void Throughput()
        {
            var config = ManualConfig.Create(DefaultConfig.Instance);
            config.Add(StatisticColumn.P90);
            config.Add(StatisticColumn.P95);

            var benchs = BenchmarkConverter.TypeToBenchmarks(typeof(ThroughputBenchmarks))
                                     .ToArray();

            BenchmarkRunner.Run(benchs, config);
        }


        private static void LatencyMultiProducer(int threadCount, int messageCount, int queueSize)
        {
            var zeroLog = new ZeroLogMultiProducer().Bench(queueSize, messageCount, threadCount);
            var nlogSync = new NLogSyncMultiProducer().Bench(messageCount, threadCount);
            var nlogAsync = new NLogAsyncMultiProducer().Bench(queueSize, messageCount, threadCount);
            var log4net = new Log4NetMultiProducer().Bench(messageCount, threadCount);

            SimpleLatencyBenchmark.PrintSummary($"{threadCount} producers, {messageCount} total log events (queue size={queueSize}) - unit is *us*",
                                                ("ZeroLog", zeroLog),
                                                ("NLogSync", nlogSync),
                                                ("NLogAsync", nlogAsync),
                                                ("Log4net", log4net));

        }

        public static void Main()
        {
            //Throughput();
            //LatencySingleProducer();

            LatencyMultiProducer(4, 4 * 25_000, 64);
            LatencyMultiProducer(8, 8 * 25_000, 64);
            LatencyMultiProducer(4, 4 * 25_000, 1024);
            LatencyMultiProducer(8, 8 * 25_000, 1024);

            Console.ReadLine();
        }
    }
}
