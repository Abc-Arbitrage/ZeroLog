using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ConsoleTables.Core;
using HdrHistogram;

namespace ZeroLog.Benchmarks.Tools
{
    public class SimpleLatencyBenchmark
    {
        public static LongHistogram Bench(Action<int> action, int count)
        {
            var histogram = new LongHistogram(TimeStamp.Minutes(1), 5);

            for (var i = 0; i < count; i++)
                histogram.Record(() => action(i));

            return histogram;
        }

        public static double Percentile(List<HistogramBase> seq, double percentile)
        {
            var result = seq.First().Copy();
            foreach(var h in seq.Skip(1))
                result.Add(h);
            return result.GetValueAtPercentile((int)(percentile * 100));
        }

        public static double Mean(List<HistogramBase> seq)
        {
            var result = seq.First().Copy();
            foreach(var h in seq.Skip(1))
                result.Add(h);
            return result.GetMean();
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
            public double Max { get; set; }
        }

        public static void PrintSummary(string title, params (string, List<HistogramBase>)[] results)
        {
            double format(double input)
            {
                return Math.Round(input, 2);
            }

            Console.WriteLine(title);
            Console.WriteLine(String.Join("", Enumerable.Range(0, title.Length).Select(_ => "=")));
            Console.WriteLine();

            ConsoleTable.From(results.Select(x =>
                        {
                            var histo = SimpleLatencyBenchmark.Concatenate(x.Item2);
                            return new Result
                            {
                                Test = x.Item1,
                                Mean = format(Math.Round(histo.GetMean())),
                                Median = format(histo.GetValueAtPercentile(50)),
                                Max = format(histo.GetMaxValue()),
                                P90 = format(histo.GetValueAtPercentile(90)),
                                P95 = format(histo.GetValueAtPercentile(95)),
                                P99 = format(histo.GetValueAtPercentile(99)),
                                P99_9 = format(histo.GetValueAtPercentile(99.9)),

                            };
                        }))
                        .Write(Format.Alternative);
        }

        private static HistogramBase Concatenate(List<HistogramBase> seq)
        {
            var result = seq.First().Copy();
            foreach (var h in seq.Skip(1))
                result.Add(h);
            return result;
        }
    }
}