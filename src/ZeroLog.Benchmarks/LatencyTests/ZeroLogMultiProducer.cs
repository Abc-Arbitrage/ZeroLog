using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HdrHistogram;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Config;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class ZeroLogMultiProducer
    {
        public List<HistogramBase> Bench(int queueSize, int warmingMessageCount, int totalMessageCount, int producingThreadCount)
        {
            var appender = new ZeroLog.Tests.TestAppender(false);
            BasicConfigurator.Configure(new[] { appender }, queueSize, logEventPoolExhaustionStrategy: LogEventPoolExhaustionStrategy.WaitForLogEvent);
            var logger = LogManager.GetLogger(nameof(ZeroLog));

            var signal = appender.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
                var warmingResult = SimpleLatencyBenchmark.Bench(i => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                return SimpleLatencyBenchmark.Bench(i => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), messageByProducer);
            });

            var tasks = new List<Task<HistogramBase>>();
            for (var i = 0; i < producingThreadCount; i++)
                tasks.Add(Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning));

            signal.Wait(TimeSpan.FromSeconds(30));
            LogManager.Shutdown();

            return tasks.Select(x => x.Result).ToList();
        }
    }
}
