using System;
using System.Threading.Tasks;
using HdrHistogram;
using NLog.Config;
using NLog.Targets.Wrappers;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests;

public class NLogAsyncMultiProducer
{
    public SimpleLatencyBenchmarkResult Bench(int queueSize, int warmingMessageCount, int totalMessageCount, int producingThreadCount)
    {
        var appender = new NLogTestTarget(false);
        var asyncTarget = new AsyncTargetWrapper(appender, queueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block);

        var config = new LoggingConfiguration();
        config.AddTarget(nameof(asyncTarget), asyncTarget);
        config.LoggingRules.Add(new LoggingRule(nameof(asyncTarget), NLog.LogLevel.Debug, asyncTarget));
        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();

        var logger = NLog.LogManager.GetLogger(nameof(asyncTarget));


        var signal = appender.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

        var produce = new Func<HistogramBase>(() =>
        {
            var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
            int[] counter = { 0 };
            var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), warmingMessageByProducer);

            var messageByProducer = totalMessageCount / producingThreadCount;
            counter[0] = 0;
            return SimpleLatencyBenchmark.Bench(() => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), messageByProducer);
        });

        var flusher = new Action(() =>
        {
            while (!signal.IsSet)
                NLog.LogManager.Flush();
        });

        Task.Factory.StartNew(flusher, TaskCreationOptions.LongRunning);

        var result = SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);
        LogManager.Shutdown();

        return result;
    }

}
