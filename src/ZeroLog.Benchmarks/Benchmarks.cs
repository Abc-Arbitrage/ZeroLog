using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using log4net.Config;
using log4net.Layout;
using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using System;
using System.Threading.Tasks;
using ZeroLog.Tests;

namespace ZeroLog.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Monitoring, warmupCount: 1, targetCount: 5, invocationCount: 5)]
    public class Benchmarks
    {
        [Params(1,4)]
        public int ProducingThreadCount;

        [Params(4 * 25_000)]
        public int TotalMessageCount;

        [Params(64, 1024)]
        public int QueueSize;

        [Benchmark]
        public void ZeroLog()
        {
            var testAppender = new TestAppender(false);
            var signal = testAppender.SetMessageCountTarget(TotalMessageCount);

            LogManager.Initialize(new[] { testAppender }, QueueSize, exhaustionStrategy: LogEventPoolExhaustionStrategy.WaitForLogEvent);
            var logger = LogManager.GetLogger(nameof(ZeroLog));

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
            LogManager.Shutdown();
        }

        [Benchmark]
        public void Log4Net()
        {
            var layout = new PatternLayout("%-4timestamp [%thread] %-5level %logger %ndc - %message%newline");
            var testAppender = new Log4NetTestAppender(false);
            var signal = testAppender.SetMessageCountTarget(TotalMessageCount);
            layout.ActivateOptions();
            testAppender.ActivateOptions();
            BasicConfigurator.Configure(testAppender);

            var logger = log4net.LogManager.GetLogger(nameof(Log4Net));

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    logger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
        }

        [Benchmark]
        public void NLogSync()
        {
            var testTarget = new NLogTestTarget(false);
            var signal = testTarget.SetMessageCountTarget(TotalMessageCount);

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(testTarget), testTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, testTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();
            var logger = NLog.LogManager.GetLogger(nameof(NLogSync));

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    logger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);

            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            NLog.LogManager.Flush();
            signal.Wait(TimeSpan.FromSeconds(30));
        }

        [Benchmark]
        public void NLogAsync()
        {
            var testTarget = new NLogTestTarget(false);
            var signal = testTarget.SetMessageCountTarget(TotalMessageCount);
            var asyncTarget = (new AsyncTargetWrapper(testTarget, QueueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block));

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(asyncTarget), asyncTarget);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, asyncTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();
            var logger = NLog.LogManager.GetLogger(nameof(NLogAsync));

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    logger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            var flusher = new Action(() =>
            {
                while(!signal.IsSet)
                    NLog.LogManager.Flush();
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(flusher, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
        }

        public static void Main()
        {
            // Setup
            NLog.Targets.Target.Register<NLogTestTarget>("NLogTestTarget");


            // Run baby run
            var summary = BenchmarkRunner.Run<Benchmarks>();
            Console.ReadLine();
        }
    }
}
