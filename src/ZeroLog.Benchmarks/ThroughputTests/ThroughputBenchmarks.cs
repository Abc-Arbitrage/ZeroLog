using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using log4net.Layout;
using NLog;
using NLog.Config;
using NLog.Targets.Wrappers;
using Serilog.Events;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;

namespace ZeroLog.Benchmarks.ThroughputTests;

[MemoryDiagnoser]
[AllStatisticsColumn]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 100, invocationCount: 1, baseline: true)]
public class ThroughputBenchmarks
{
    [Params(4)]
    public int ProducingThreadCount;

    [Params(4 * 50_000)]
    public int TotalMessageCount;

    [Params(8192)]
    //[Params(4 * 50_000)]
    public int QueueSize;

    // ZeroLog
    private Tests.TestAppender _zeroLogTestAppender;
    private Log _zeroLogLogger;

    // Log4Net
    private log4net.ILog _log4NetLogger;
    private Log4NetTestAppender _log4NetTestAppender;

    // NLog
    private NLogTestTarget _nLogTestTarget;
    private Logger _nLogLogger;

    // Serilog
    private SerilogTestSink _serilogTestSink;
    private Serilog.Core.Logger _serilogLogger;

    //
    // ZeroLog
    //

    [GlobalSetup(Target = nameof(ZeroLog_Default))]
    public void SetupZeroLog_Default()
        => SetupZeroLog(LogMessagePoolExhaustionStrategy.Default);

    [GlobalSetup(Target = nameof(ZeroLog_WaitUntilAvailable))]
    public void SetupZeroLog_WaitUntilAvailable()
        => SetupZeroLog(LogMessagePoolExhaustionStrategy.WaitUntilAvailable);

    private void SetupZeroLog(LogMessagePoolExhaustionStrategy strategy)
    {
        if (LogManager.Configuration is not null)
            throw new InvalidOperationException();

        _zeroLogTestAppender = new Tests.TestAppender(false);

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = QueueSize,
            RootLogger =
            {
                LogMessagePoolExhaustionStrategy = strategy,
                Appenders = { _zeroLogTestAppender }
            }
        });

        _zeroLogLogger = LogManager.GetLogger("ZeroLog");
    }

    [GlobalCleanup(Targets = [nameof(ZeroLog_Default), nameof(ZeroLog_WaitUntilAvailable)])]
    public void CleanupZeroLog()
        => LogManager.Shutdown();

    [Benchmark]
    public void ZeroLog_Default()
    {
        // Note: This one is (very) unfair to the others

        var produce = new Action(() =>
        {
            for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
            {
                var text = "dude";
                _zeroLogLogger.Info($"Hi {text} ! It's {DateTime.UtcNow:HH:mm:ss}, and the message is #{i}");
            }
        });

        Task.WaitAll(
            Enumerable.Range(0, ProducingThreadCount).Select(_ => Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning))
        );

        LogManager.Flush();
    }

    [Benchmark(Baseline = true)]
    public void ZeroLog_WaitUntilAvailable()
    {
        var signal = _zeroLogTestAppender.SetMessageCountTarget(TotalMessageCount);

        var produce = new Action(() =>
        {
            for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
            {
                var text = "dude";
                _zeroLogLogger.Info($"Hi {text} ! It's {DateTime.UtcNow:HH:mm:ss}, and the message is #{i}");
            }
        });

        for (var i = 0; i < ProducingThreadCount; i++)
            Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

        signal.Wait(TimeSpan.FromSeconds(30));
    }

    //
    // Log4Net
    //

    [GlobalSetup(Target = nameof(Log4Net))]
    public void SetupLog4Net()
    {
        var layout = new PatternLayout("%-4timestamp [%thread] %-5level %logger %ndc - %message%newline");
        _log4NetTestAppender = new Log4NetTestAppender(false);
        layout.ActivateOptions();
        _log4NetTestAppender.ActivateOptions();

        var repository = log4net.LogManager.GetRepository(Assembly.GetExecutingAssembly());
        log4net.Config.BasicConfigurator.Configure(repository, _log4NetTestAppender);

        _log4NetLogger = log4net.LogManager.GetLogger(repository.Name, nameof(Log4Net));
    }

    [GlobalCleanup(Target = nameof(Log4Net))]
    public void CleanupLog4Net()
        => log4net.LogManager.Shutdown();

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

    [GlobalSetup(Target = nameof(NLog_Sync))]
    public void SetupLogNLog_Sync()
    {
        _nLogTestTarget = new NLogTestTarget(false);

        var config = new LoggingConfiguration();
        config.AddTarget(nameof(_nLogTestTarget), _nLogTestTarget);
        config.LoggingRules.Add(new LoggingRule(nameof(NLog_Sync), NLog.LogLevel.Debug, _nLogTestTarget));
        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();

        _nLogLogger = NLog.LogManager.GetLogger(nameof(NLog_Sync));
    }

    [GlobalSetup(Target = nameof(NLog_Async))]
    public void SetupLogNLog_Async()
    {
        _nLogTestTarget = new NLogTestTarget(false);
        var asyncTarget = new AsyncTargetWrapper(_nLogTestTarget, QueueSize, overflowAction: AsyncTargetWrapperOverflowAction.Block);

        var config = new LoggingConfiguration();
        config.AddTarget(nameof(_nLogTestTarget), asyncTarget);
        config.LoggingRules.Add(new LoggingRule(nameof(NLog_Async), NLog.LogLevel.Debug, asyncTarget));
        NLog.LogManager.Configuration = config;
        NLog.LogManager.ReconfigExistingLoggers();

        _nLogLogger = NLog.LogManager.GetLogger(nameof(NLog_Async));
    }

    [GlobalCleanup(Targets = [nameof(NLog_Sync), nameof(NLog_Async)])]
    public void CleanupNLog()
        => NLog.LogManager.Shutdown();

    [Benchmark]
    public void NLog_Sync()
    {
        var signal = _nLogTestTarget.SetMessageCountTarget(TotalMessageCount);

        var produce = new Action(() =>
        {
            for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                _nLogLogger.Debug("Hi {Name} ! It's {Hour:HH:mm:ss}, and the message is #{Number}", "dude", DateTime.UtcNow, i);
        });

        for (var i = 0; i < ProducingThreadCount; i++)
            Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

        signal.Wait(TimeSpan.FromSeconds(30));
    }

    [Benchmark]
    public void NLog_Async()
    {
        var signal = _nLogTestTarget.SetMessageCountTarget(TotalMessageCount);

        var produce = new Action(() =>
        {
            for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                _nLogLogger.Debug("Hi {Name} ! It's {Hour:HH:mm:ss}, and the message is #{Number}", "dude", DateTime.UtcNow, i);
        });

        for (var i = 0; i < ProducingThreadCount; i++)
            Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

        signal.Wait(TimeSpan.FromSeconds(30));
    }

    //
    // Serilog
    //

    [GlobalSetup(Target = nameof(Serilog))]
    public void SetupSerilog()
    {
        _serilogTestSink = new SerilogTestSink(false);

        _serilogLogger = new Serilog.LoggerConfiguration()
                         .WriteTo.Sink(_serilogTestSink)
                         .CreateLogger();

        if (!_serilogLogger.IsEnabled(LogEventLevel.Information))
            throw new InvalidOperationException();
    }

    [GlobalCleanup(Target = nameof(Serilog))]
    public void CleanupSerilog()
        => _serilogLogger.Dispose();

    [Benchmark]
    public void Serilog()
    {
        var signal = _serilogTestSink.SetMessageCountTarget(TotalMessageCount);

        var produce = new Action(() =>
        {
            for (var i = 0; i < TotalMessageCount / ProducingThreadCount; i++)
                _serilogLogger.Information("Hi {Name} ! It's {Hour:HH:mm:ss}, and the message is #{Number}", "dude", DateTime.UtcNow, i);
        });

        for (var i = 0; i < ProducingThreadCount; i++)
            Task.Factory.StartNew(produce, TaskCreationOptions.LongRunning);

        signal.Wait(TimeSpan.FromSeconds(30));
    }
}
