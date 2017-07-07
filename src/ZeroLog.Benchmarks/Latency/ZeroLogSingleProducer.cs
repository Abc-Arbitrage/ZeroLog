using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Benchmarks.Latency
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 5000, invocationCount: 1)]
    public class ZeroLogSingleProducer
    {
        [Params(5000)]
        public int TotalMessageCount;

        [Params(512)]
        public int QueueSize;

        private Tests.TestAppender _zeroLogTestAppender;
        private ILog _zeroLogLogger;
        private ManualResetEventSlim _signal;


        [GlobalSetup]
        public void Setup()
        {
            _zeroLogTestAppender = new ZeroLog.Tests.TestAppender(false);

            BasicConfigurator.Configure(new[] { _zeroLogTestAppender }, QueueSize, logEventPoolExhaustionStrategy: LogEventPoolExhaustionStrategy.WaitForLogEvent);
            _zeroLogLogger = LogManager.GetLogger(nameof(ZeroLog));

            _signal = _zeroLogTestAppender.SetMessageCountTarget(TotalMessageCount);
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _signal.Wait(TimeSpan.FromSeconds(30));
            LogManager.Shutdown();
        }

        [Benchmark]
        public void Log()
        {
            _zeroLogLogger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, 112);
        }       
    }
}
