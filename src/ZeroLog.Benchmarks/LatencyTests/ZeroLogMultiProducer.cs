using System;
using HdrHistogram;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Config;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class ZeroLogMultiProducer
    {
        public SimpleLatencyBenchmarkResult Bench(int queueSize, int warmingMessageCount, int totalMessageCount, int producingThreadCount)
        {
            var appender = new Tests.TestAppender(false);
            BasicConfigurator.Configure(new[] { appender }, queueSize, logEventPoolExhaustionStrategy: LogEventPoolExhaustionStrategy.WaitForLogEvent);
            var logger = LogManager.GetLogger(nameof(ZeroLog));

            var signal = appender.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
                int[] counter = { 0 };
                var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                counter[0] = 0;
                return SimpleLatencyBenchmark.Bench(() => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), messageByProducer);
            });

            var result = SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);
            LogManager.Shutdown();

            return result;
        }
    }
}
