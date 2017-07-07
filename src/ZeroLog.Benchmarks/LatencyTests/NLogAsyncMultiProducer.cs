using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HdrHistogram;
using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class NLogAsyncMultiProducer
    {
        public List<HistogramBase> Bench(int queueSize, int warmingMessageCount, int totalMessageCount, int producingThreadCount)
        {
            var appender = new NLogTestTarget(false);
            var asyncTarget = (new AsyncTargetWrapper(appender, queueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block));

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(asyncTarget), asyncTarget);
            config.LoggingRules.Add(new LoggingRule(nameof(asyncTarget), LogLevel.Debug, asyncTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();

            var logger = NLog.LogManager.GetLogger(nameof(asyncTarget));


            var signal = appender.SetMessageCountTarget(warmingMessageCount + totalMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var warmingMessageByProducer = warmingMessageCount / producingThreadCount;
                var warmingResult = SimpleLatencyBenchmark.Bench(i => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                return SimpleLatencyBenchmark.Bench(i => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), messageByProducer);
            });

            var flusher = new Action(() =>
            {
                while (!signal.IsSet)
                    NLog.LogManager.Flush();
            });

            Task.Factory.StartNew(flusher, TaskCreationOptions.LongRunning);

            var tasks = new List<Task<HistogramBase>>();
            for (var i = 0; i < producingThreadCount; i++)
                tasks.Add(Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning));

            signal.Wait(TimeSpan.FromSeconds(30));
            LogManager.Shutdown();

            return tasks.Select(x => x.Result).ToList();
        }

    }
}
