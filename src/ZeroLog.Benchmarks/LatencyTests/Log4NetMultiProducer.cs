using System;
using HdrHistogram;
using log4net.Config;
using log4net.Layout;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class Log4NetMultiProducer
    {
        public SimpleLatencyBenchmarkResult Bench(int warmingMessageCount, int totalMessageCount, int producingThreadCount)
        {
            var layout = new PatternLayout("%-4timestamp [%thread] %-5level %logger %ndc - %message%newline");
            layout.ActivateOptions();
            var appender = new Log4NetTestAppender(false);
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);

            var logger = log4net.LogManager.GetLogger(nameof(appender));


            var signal = appender.SetMessageCountTarget(totalMessageCount + warmingMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
                int[] counter = { 0 };
                var warmingResult = SimpleLatencyBenchmark.Bench(() => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                counter[0] = 0;
                return SimpleLatencyBenchmark.Bench(() => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, counter[0]++), messageByProducer);
            });

            return SimpleLatencyBenchmark.RunBench(producingThreadCount, produce, signal);
        }

    }
}
