using System;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using log4net.Layout;
using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using ZeroLog.Config;
using BasicConfigurator = ZeroLog.Config.BasicConfigurator;

namespace ZeroLog.Benchmarks.ThroughputTests
{
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart, targetCount: 1000, invocationCount: 1)]
    public class ThroughputBenchmarks
    {
        [Params(4)]
        public int ProducingThreadCount;

        [Params(4 * 5_000)]
        public int TotalMessageCount;

        [Params(512)]
        public int QueueSize;

        // ZeroLog
        private ZeroLog.Tests.TestAppender _zeroLogTestAppender;
        private ILog _zeroLogLogger;

        // Log4Net
        private log4net.ILog _log4NetLogger;
        private Log4NetTestAppender _log4NetTestAppender;

        // NLog
        private NLogTestTarget _nLogTestTarget;
        private Logger _nLogLogger;
        private NLogTestTarget _nLogAsyncTestTarget;
        private Logger _nLogAsyncLogger;

        [GlobalSetup]
        public void Setup()
        {
            SetupZeroLog();
            SetupLog4Net();
            SetupLogNLog();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            TearDownZeroLog();
            TearDownLog4Net();
            TearDownNLog();
        }

        //
        // ZeroLog
        //

        private void SetupZeroLog()
        {
            _zeroLogTestAppender = new ZeroLog.Tests.TestAppender(false);

            BasicConfigurator.Configure(new ZeroLogBasicConfig
            {
                Appenders = { _zeroLogTestAppender },
                LogEventQueueSize = QueueSize,
                LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.WaitForLogEvent
            });

            _zeroLogLogger = LogManager.GetLogger(nameof(ZeroLog));
        }

        private void TearDownZeroLog()
        {
            LogManager.Shutdown();
        }


        [Benchmark]
        public void ZeroLog()
        {
            var signal = _zeroLogTestAppender.SetMessageCountTarget(TotalMessageCount);

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    _zeroLogLogger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
        }


        //
        // Log4Net
        //

        private void SetupLog4Net()
        {
            var layout = new PatternLayout("%-4timestamp [%thread] %-5level %logger %ndc - %message%newline");
            _log4NetTestAppender = new Log4NetTestAppender(false);
            layout.ActivateOptions();
            _log4NetTestAppender.ActivateOptions();

            var repository = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
            log4net.Config.BasicConfigurator.Configure(repository, _log4NetTestAppender);

            _log4NetLogger = log4net.LogManager.GetLogger(repository.Name, nameof(Log4Net));
        }

        private void TearDownLog4Net()
        {
        }

        [Benchmark]
        public void Log4Net()
        {
            var signal = _log4NetTestAppender.SetMessageCountTarget(TotalMessageCount);


            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    _log4NetLogger.InfoFormat("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
        }


        //
        // NLog Sync
        //

        private void SetupLogNLog()
        {
            _nLogTestTarget = new NLogTestTarget(false);
            _nLogAsyncTestTarget = new NLogTestTarget(false);
            var asyncTarget = (new AsyncTargetWrapper(_nLogAsyncTestTarget, QueueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block));

            var config = new LoggingConfiguration();
            config.AddTarget(nameof(_nLogTestTarget), _nLogTestTarget);
            config.AddTarget(nameof(asyncTarget), asyncTarget);
            config.LoggingRules.Add(new LoggingRule(nameof(NLogSync), LogLevel.Debug, _nLogTestTarget));
            config.LoggingRules.Add(new LoggingRule(nameof(NLogAsync), LogLevel.Debug, asyncTarget));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ReconfigExistingLoggers();

            _nLogLogger = NLog.LogManager.GetLogger(nameof(NLogSync));
            _nLogAsyncLogger = NLog.LogManager.GetLogger(nameof(NLogAsync));
        }

        private void TearDownNLog()
        {
            
        }

        [Benchmark]
        public void NLogSync()
        {
            var signal = _nLogTestTarget.SetMessageCountTarget(TotalMessageCount);

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    _nLogLogger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);

            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            NLog.LogManager.Flush();
            signal.Wait(TimeSpan.FromSeconds(30));
        }


        [Benchmark]
        public void NLogAsync()
        {
            var signal = _nLogAsyncTestTarget.SetMessageCountTarget(TotalMessageCount);

            var produce = new Action(() =>
            {
                for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                    _nLogAsyncLogger.Debug("Hi {0} ! It's {1:HH:mm:ss}, and the message is #{2}", "dude", DateTime.UtcNow, i);
            });

            var flusher = new Action(() =>
            {
                while (!signal.IsSet)
                    NLog.LogManager.Flush();
            });

            for (var i = 0; i < ProducingThreadCount; i++)
                Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(flusher, TaskCreationOptions.LongRunning);

            signal.Wait(TimeSpan.FromSeconds(30));
        }
    }
}
