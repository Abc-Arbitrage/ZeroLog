using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using ZeroLog;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;
using ZeroLog.Tests;
using ZLogger;

// ReSharper disable once CheckNamespace
namespace Benchmarks;

[MemoryDiagnoser]
[AllStatisticsColumn]
[SimpleJob(iterationCount: 32, invocationCount: 128)]
public partial class LatencyBenchmarks
{
    private const int _operationCount = 8 * 1024;

    private static readonly string _text = "dude";
    private static readonly DateTime _date = DateTime.UtcNow;
    private static readonly int _number = 42;

    // ZeroLog
    private TestAppender _zeroLogTestAppender;
    private Log _zeroLogLogger;

    // ZLogger
    private ILoggerFactory _zLoggerFactory;
    private ILogger _zLoggerLogger;

    // Serilog
    private SerilogTestSink _serilogTestSink;
    private Serilog.Core.Logger _serilogLogger;

    [ParamsAllValues]
    public bool Enabled { get; [UsedImplicitly] set; }

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

        _zeroLogTestAppender = new TestAppender(false);

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = _operationCount,
            RootLogger =
            {
                Level = Enabled ? ZeroLog.LogLevel.Info : ZeroLog.LogLevel.Warn,
                LogMessagePoolExhaustionStrategy = strategy,
                Appenders = { _zeroLogTestAppender }
            }
        });

        _zeroLogLogger = LogManager.GetLogger("ZeroLog");

        if (_zeroLogLogger.IsInfoEnabled != Enabled)
            throw new InvalidOperationException();
    }

    [GlobalCleanup(Targets = [nameof(ZeroLog_Default), nameof(ZeroLog_WaitUntilAvailable)])]
    public void CleanupZeroLog()
        => LogManager.Shutdown();

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void ZeroLog_Default()
    {
        // This strategy is unfair to the other ones, as it allows skipping messages, but it is the default one, so bench it anyway.
        for (var i = 0; i < _operationCount; ++i)
            _zeroLogLogger.Info($"Hi {_text}! It's {_date:HH:mm:ss}, and the message is #{_number}");
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = _operationCount)]
    public void ZeroLog_WaitUntilAvailable()
    {
        for (var i = 0; i < _operationCount; ++i)
            _zeroLogLogger.Info($"Hi {_text}! It's {_date:HH:mm:ss}, and the message is #{_number}");
    }

    [IterationCleanup(Targets = [nameof(ZeroLog_Default), nameof(ZeroLog_WaitUntilAvailable)])]
    public void CleanupZeroLogIteration()
        => LogManager.Flush();

    //
    // ZLogger
    //

    [GlobalSetup(Targets = [nameof(ZLogger_Standard), nameof(ZLogger_Generated)])]
    public void SetupZLogger()
    {
        _zLoggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddZLoggerStream(Stream.Null);
            logging.SetMinimumLevel(Enabled ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        _zLoggerLogger = _zLoggerFactory.CreateLogger(nameof(ZLogger_Standard));

        if (_zLoggerLogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information) != Enabled)
            throw new InvalidOperationException();
    }

    [GlobalCleanup(Targets = [nameof(ZLogger_Standard), nameof(ZLogger_Generated)])]
    public void CleanupZLogger()
        => _zLoggerFactory.Dispose();

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void ZLogger_Standard()
    {
        for (var i = 0; i < _operationCount; ++i)
            _zLoggerLogger.ZLogInformation($"Hi {_text}! It's {_date:HH:mm:ss}, and the message is #{_number}");
    }

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void ZLogger_Generated()
    {
        for (var i = 0; i < _operationCount; ++i)
            ZLogger_Generated(_zLoggerLogger, _text, _date, _number);
    }

    [ZLoggerMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Hi {name}! It's {hour:HH:mm:ss}, and the message is #{number}")]
    private static partial void ZLogger_Generated(ILogger logger, string name, DateTime hour, int number);

    //
    // Serilog
    //

    [GlobalSetup(Target = nameof(Serilog))]
    public void SetupSerilog()
    {
        _serilogTestSink = new SerilogTestSink(false);

        _serilogLogger = new Serilog.LoggerConfiguration()
                         .WriteTo.Sink(_serilogTestSink)
                         .MinimumLevel.Is(Enabled ? LogEventLevel.Information : LogEventLevel.Warning)
                         .CreateLogger();

        if (_serilogLogger.IsEnabled(LogEventLevel.Information) != Enabled)
            throw new InvalidOperationException();
    }

    [GlobalCleanup(Target = nameof(Serilog))]
    public void CleanupSerilog()
        => _serilogLogger.Dispose();

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void Serilog()
    {
        for (var i = 0; i < _operationCount; ++i)
            _serilogLogger.Information("Hi {Name}! It's {Hour:HH:mm:ss}, and the message is #{Number}", _text, _date, _number);
    }
}
