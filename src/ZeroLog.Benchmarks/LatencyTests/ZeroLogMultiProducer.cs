using System;
using HdrHistogram;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;
using ZeroLog.Tests;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class ZeroLogMultiProducer
    {
        public SimpleLatencyBenchmarkResult Bench(int queueSize, int warmingMessageCount, int totalMessageCount, int producingThreadCount)
        {
            var appender = new TestAppender(false);
            LogManager.Initialize(new ZeroLogConfiguration
            {
                RootLogger =
                {
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                    Appenders = {appender}
                },
                LogMessagePoolSize = queueSize,
            });
            var logger = LogManager.GetLogger(nameof(ZeroLog));

            var signal = appender.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
                int[] counter = { 0 };
                const string text = "dude";
                var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.Info($"Hi {text} ! It's {DateTime.UtcNow:HH:mm:ss}, and the message is #{counter[0]++}"), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                counter[0] = 0;
                return SimpleLatencyBenchmark.Bench(() => logger.Info($"Hi {text} ! It's {DateTime.UtcNow:HH:mm:ss}, and the message is #{counter[0]++}"), messageByProducer);
            });

            var result = SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);
            LogManager.Shutdown();

            return result;
        }
    }
}
