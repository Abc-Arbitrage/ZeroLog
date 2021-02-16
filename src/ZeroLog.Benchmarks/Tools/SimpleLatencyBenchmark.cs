using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public static void PrintSummary(string title, params (string name, SimpleLatencyBenchmarkResult result)[] results)
        {
            Console.WriteLine(title);
            Console.WriteLine(String.Join("", Enumerable.Range(0, title.Length).Select(_ => "=")));
            Console.WriteLine();

            Console.WriteLine("+------------+--------+--------+--------+------------+------------+------------+------------+------------+------------+------------+");
            Console.WriteLine("| Test       |   Mean | Median |    P90 |        P95 |        P99 |      P99.9 |     P99.99 |    P99.999 |        Max |   GC Count |");
            Console.WriteLine("+------------+--------+--------+--------+------------+------------+------------+------------+------------+------------+------------+");

            foreach (var (name, result) in results)
            {
                var histo = Concatenate(result.ExecutionTimes);

                Console.WriteLine($"| {name,-10} | {histo.GetMean(),6:N0} | {histo.GetValueAtPercentile(50),6:N0} | {histo.GetValueAtPercentile(90),6:N0} | {histo.GetValueAtPercentile(95),10:N0} | {histo.GetValueAtPercentile(99),10:N0} | {histo.GetValueAtPercentile(99.9),10:N0} | {histo.GetValueAtPercentile(99.99),10:N0} | {histo.GetValueAtPercentile(99.999),10:N0} | {histo.GetMaxValue(),10:N0} | {result.CollectionCount,10:N0} |");
            }

            Console.WriteLine("+------------+--------+--------+--------+------------+------------+------------+------------+------------+------------+------------+");
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
