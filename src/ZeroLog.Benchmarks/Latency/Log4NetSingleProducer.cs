using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Engines;
using log4net.Config;
using log4net.Layout;

namespace ZeroLog.Benchmarks.Latency
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 5000, invocationCount: 1)]
    public class Log4NetSingleProducer
    {
        [Params(5000)]
        public int TotalMessageCount;

        private Log4NetTestAppender _log4NetTestAppender;
        private log4net.ILog _log4NetLogger;
        private ManualResetEventSlim _signal;

        [GlobalSetup]
        public void Setup()
        {
            var layout = new PatternLayout("%-4timestamp [%thread] %-5level %logger %ndc - %message%newline");
            _log4NetTestAppender = new Log4NetTestAppender(false);
            layout.ActivateOptions();
            _log4NetTestAppender.ActivateOptions();
            BasicConfigurator.Configure(_log4NetTestAppender);

            _log4NetLogger = log4net.LogManager.GetLogger(nameof(Log4NetSingleProducer));

            _signal = _log4NetTestAppender.SetMessageCountTarget(TotalMessageCount);
        }

        [GlobalCleanup]
        public void TearDown()
        {
            _signal.Wait(TimeSpan.FromSeconds(30));
        }

        [Benchmark]
        public void Log()
        {
            _log4NetLogger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, 112);
        }
    }
}
