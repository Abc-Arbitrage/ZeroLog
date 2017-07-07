using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HdrHistogram;
using log4net.Config;
using log4net.Layout;
using ZeroLog.Benchmarks.Tools;

namespace ZeroLog.Benchmarks.LatencyTests
{
    public class Log4NetMultiProducer
    {
        public List<HistogramBase> Bench(int warmingMessageCount, int totalMessageCount, int producingThreadCount)
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
                var warmingResult = SimpleLatencyBenchmark.Bench(i => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), warmingMessageByProducer);

                var messageByProducer = totalMessageCount / producingThreadCount;
                return SimpleLatencyBenchmark.Bench(i => logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i), messageByProducer);
            });

            var tasks = new List<Task<HistogramBase>>();
            for (var i = 0; i < producingThreadCount; i++)
                tasks.Add(Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning));

            signal.Wait(TimeSpan.FromSeconds(30));

            return tasks.Select(x => x.Result).ToList();
        }

    }
}
