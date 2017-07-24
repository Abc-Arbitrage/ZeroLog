using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTables;
using HdrHistogram;

namespace ZeroLog.Benchmarks.Tools
{
    public class SimpleLatencyBenchmark
    {
        public static LongHistogram Bench(Action action, int count)
        {
            var histogram = new LongHistogram(TimeStamp.Minutes(1), 5);

            for (var i = 0; i < count; i++)
                histogram.Record(() => action());

            return histogram;
        }

        struct Result
        {
            public string Test { get; set; }
            public double Mean { get; set; }
            public double Median { get; set; }
            public double P90 { get; set; }
            public double P95 { get; set; }
            public double P99 { get; set; }
            public double P99_9 { get; set; }
            public double P99_99 { get; set; }
            public double P99_999 { get; set; }
            public double Max { get; set; }
            public int GCCount { get; set; }
        }

        public static void PrintSummary(string title, params (string, SimpleLatencyBenchmarkResult)[] results)
        {
            double Format(double input)
            {
                return Math.Round(input, 2);
            }

            Console.WriteLine(title);
            Console.WriteLine(String.Join("", Enumerable.Range(0, title.Length).Select(_ => "=")));
            Console.WriteLine();

            ConsoleTable.From(results.Select(x =>
                        {
                            var histo = Concatenate(x.Item2.ExecutionTimes);
                            return new Result
                            {
                                Test = x.Item1,
                                Mean = Format(Math.Round(histo.GetMean())),
                                Median = Format(histo.GetValueAtPercentile(50)),
                                Max = Format(histo.GetMaxValue()),
                                P90 = Format(histo.GetValueAtPercentile(90)),
                                P95 = Format(histo.GetValueAtPercentile(95)),
                                P99 = Format(histo.GetValueAtPercentile(99)),
                                P99_9 = Format(histo.GetValueAtPercentile(99.9)),
                                P99_99 = Format(histo.GetValueAtPercentile(99.99)),
                                P99_999 = Format(histo.GetValueAtPercentile(99.999)),
                                GCCount = x.Item2.CollectionCount,
                            };
                        }))
                        .Write(ConsoleTables.Format.Alternative);
        }

        private static HistogramBase Concatenate(List<HistogramBase> seq)
        {
            var result = seq.First().Copy();
            foreach (var h in seq.Skip(1))
                result.Add(h);
            return result;
        }

        public static SimpleLatencyBenchmarkResult RunBench(int producingThreadCount, Func<HistogramBase> produce, ManualResetEventSlim signal)
        {
            var tasks = Enumerable.Range(0, producingThreadCount).Select(_ => new Task<HistogramBase>(produce, TaskCreationOptions.LongRunning)).ToList();

            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
            var collectionsBefore = GC.CollectionCount(0);

            foreach (var task in tasks)
            {
                task.Start();
            }

            signal.Wait(TimeSpan.FromSeconds(30));

            var collectionsAfter = GC.CollectionCount(0);
            var result = new SimpleLatencyBenchmarkResult { ExecutionTimes = tasks.Select(x => x.Result).ToList(), CollectionCount = collectionsAfter - collectionsBefore };
            return result;
        }
    }

    public class SimpleLatencyBenchmarkResult
    {
        public List<HistogramBase> ExecutionTimes { get; set; }
        public int CollectionCount { get; set; }
    }
}