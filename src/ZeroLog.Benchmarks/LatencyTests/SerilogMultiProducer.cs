using System;
using HdrHistogram;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;
using ZeroLog.Tests;

namespace ZeroLog.Benchmarks.LatencyTests;

public class SerilogMultiProducer
{
    public SimpleLatencyBenchmarkResult Bench(int warmingMessageCount, int totalMessageCount, int producingThreadCount)
    {
        var sink = new SerilogTestSink(false);

        var logger = new Serilog.LoggerConfiguration()
                     .WriteTo.Sink(sink)
                     .CreateLogger();

        var signal = sink.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

        var produce = new Func<HistogramBase>(() =>
        {
            var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
            int[] counter = { 0 };
            var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.Information("Hi {name} ! It's {date:HH:mm:ss}, and the message is #{number}", "dude", DateTime.UtcNow, counter[0]++), warmingMessageByProducer);

            var messageByProducer = totalMessageCount / producingThreadCount;
            counter[0] = 0;
            return SimpleLatencyBenchmark.Bench(() => logger.Information("Hi {name} ! It's {date:HH:mm:ss}, and the message is #{number}", "dude", DateTime.UtcNow, counter[0]++), messageByProducer);
        });

        var result = SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);

        return result;
    }
}
