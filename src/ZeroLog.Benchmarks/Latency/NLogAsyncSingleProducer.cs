using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;

namespace ZeroLog.Benchmarks.Latency
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 5000, invocationCount: 1)]
    public class NLogAsyncSingleProducer
    {
        [Params(5000)]
        public int TotalMessageCount;

        [Params(512)]
        public int QueueSize;

        private NLogTestTarget _nLogAsyncTestTarget;
        private Logger _nLogAsyncLogger;

        private ManualResetEventSlim _signal;

        [GlobalSetup]
        public void Setup()
        {
            _nLogAsyncTestTarget = new NLogTestTarget(false);
            var asyncTarget = (new AsyncTargetWrapper(_nLogAsyncTestTarget, QueueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block));

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(asyncTarget), asyncTarget);
            config.LoggingRules.Add(new LoggingRule(nameof(NLogAsyncSingleProducer), LogLevel.Debug, asyncTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();

            _nLogAsyncLogger = NLog.LogManager.GetLogger(nameof(NLogAsyncSingleProducer));
            _signal = _nLogAsyncTestTarget.SetMessageCountTarget(TotalMessageCount);

            var flusher = new Action(() =>
            {
                while (!_signal.IsSet)
                    NLog.LogManager.Flush();
            });
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _signal.Wait(TimeSpan.FromSeconds(30));
        }

        [Benchmark]
        public void Log()
        {
            _nLogAsyncLogger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, 42);
        }
    }
}
