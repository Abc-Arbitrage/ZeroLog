using System;
using HdrHistogram;
using NLog;
using NLog.Config;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests;

public class NLogSyncMultiProducer
{
    public SimpleLatencyBenchmarkResult Bench(int warmingMessageCount, int totalMessageCount, int producingThreadCount)
    {
        var appender = new NLogTestTarget(false);

        var config = new LoggingConfiguration();
        config.AddTarget(nameof(appender), appender);
        config.LoggingRules.Add(new LoggingRule(nameof(appender), NLog.LogLevel.Debug, appender));
        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();

        var logger = NLog.LogManager.GetLogger(nameof(appender));


        var signal = appender.SetMessageCountTarget(totalMessageCount);

        var produce = new Func<HistogramBase>(() =>
        {
            var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
            int[] counter = { 0 };
            var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), warmingMessageByProducer);

            var messageByProducer = totalMessageCount / producingThreadCount;
            counter[0] = 0;
            return SimpleLatencyBenchmark.Bench(() => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), messageByProducer);
        });

        var result = SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);
        LogManager.Shutdown();

        return result;
    }

}
