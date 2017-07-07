using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HdrHistogram;
using NLog;
using NLog.Config;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class NLogSyncMultiProducer
    {
        public List<HistogramBase> Bench(int totalMessageCount, int producingThreadCount)
        {
            var appender = new NLogTestTarget(false);

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(appender), appender);
            config.LoggingRules.Add(new LoggingRule(nameof(appender), LogLevel.Debug, appender));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();

            var logger = NLog.LogManager.GetLogger(nameof(appender));


            var signal = appender.SetMessageCountTarget(totalMessageCount);

            var produce = new Func<HistogramBase>(() =>
            {
                var messageByProducer = totalMessageCount / producingThreadCount;
                return SimpleLatencyBenchmark.Bench(i => logger.Info("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), messageByProducer);
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
