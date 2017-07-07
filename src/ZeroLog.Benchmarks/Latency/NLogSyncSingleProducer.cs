using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using NLog;
using NLog.Config;

namespace ZeroLog.Benchmarks.Latency
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 5000, invocationCount: 1)]
    public class NLogSyncSingleProducer
    {
        [Params(5000)]
        public int TotalMessageCount;

        private NLogTestTarget _nLogTestTarget;
        private Logger _nLogLogger;

        private ManualResetEventSlim _signal;

        [GlobalSetup]
        public void Setup()
        {
            _nLogTestTarget = new NLogTestTarget(false);

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(_nLogTestTarget), _nLogTestTarget);
            config.LoggingRules.Add(new LoggingRule(nameof(NLogSyncSingleProducer), LogLevel.Debug, _nLogTestTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();

            _nLogLogger = NLog.LogManager.GetLogger(nameof(NLogSyncSingleProducer));
            _signal = _nLogTestTarget.SetMessageCountTarget(TotalMessageCount);
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _signal.Wait(TimeSpan.FromSeconds(30));
        }

        [Benchmark]
        public void Log()
        {
            _nLogLogger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, 42);
        }
    }
}
