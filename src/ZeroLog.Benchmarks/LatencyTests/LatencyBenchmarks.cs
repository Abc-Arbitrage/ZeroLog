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
[SimpleJob(iterationCount: 64, invocationCount: 128)]
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

    [GlobalSetup]
    public void Setup()
    {
        SetupZeroLog();
        SetupZLogger();
        SetupSerilog();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        TearDownZeroLog();
        TearDownZLogger();
        TearDownSerilog();
    }

    //
    // ZeroLog
    //

    private void SetupZeroLog()
    {
        _zeroLogTestAppender = new TestAppender(false);

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = _operationCount,
            RootLogger =
            {
                Level = Enabled ? global::ZeroLog.LogLevel.Info : global::ZeroLog.LogLevel.Warn,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                Appenders = { _zeroLogTestAppender }
            }
        });

        _zeroLogLogger = LogManager.GetLogger(nameof(ZeroLog));

        if (_zeroLogLogger.IsInfoEnabled != Enabled)
            throw new InvalidOperationException();
    }

    private void TearDownZeroLog()
        => LogManager.Shutdown();

    [Benchmark(Baseline = true, OperationsPerInvoke = _operationCount)]
    public void ZeroLog()
    {
        for (var i = 0; i < _operationCount; ++i)
            _zeroLogLogger.Info($"Hi {_text}! It's {_date:HH:mm:ss}, and the message is #{_number}");
    }

    [IterationCleanup(Target = nameof(ZeroLog))]
    public void CleanupZeroLogIteration()
        => LogManager.Flush();

    //
    // ZLogger
    //

    private void SetupZLogger()
    {
        _zLoggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddZLoggerStream(Stream.Null);
            logging.SetMinimumLevel(Enabled ? Microsoft.Extensions.Logging.LogLevel.Information : Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        _zLoggerLogger = _zLoggerFactory.CreateLogger(nameof(ZLogger));

        if (_zLoggerLogger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information) != Enabled)
            throw new InvalidOperationException();
    }

    private void TearDownZLogger()
        => _zLoggerFactory.Dispose();

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void ZLogger()
    {
        for (var i = 0; i < _operationCount; ++i)
            _zLoggerLogger.ZLogInformation($"Hi {_text}! It's {_date:HH:mm:ss}, and the message is #{_number}");
    }

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void ZLoggerGenerated()
    {
        for (var i = 0; i < _operationCount; ++i)
            ZLoggerGenerated(_zLoggerLogger, _text, _date, _number);
    }

    [ZLoggerMessage(Microsoft.Extensions.Logging.LogLevel.Information, "Hi {name}! It's {hour:HH:mm:ss}, and the message is #{number}")]
    private static partial void ZLoggerGenerated(ILogger logger, string name, DateTime hour, int number);

    //
    // Serilog
    //

    private void SetupSerilog()
    {
        _serilogTestSink = new SerilogTestSink(false);

        _serilogLogger = new Serilog.LoggerConfiguration()
                         .WriteTo.Sink(_serilogTestSink)
                         .MinimumLevel.Is(Enabled ? LogEventLevel.Information : LogEventLevel.Warning)
                         .CreateLogger();

        if (_serilogLogger.IsEnabled(LogEventLevel.Information) != Enabled)
            throw new InvalidOperationException();
    }

    private void TearDownSerilog()
        => _serilogLogger.Dispose();

    [Benchmark(OperationsPerInvoke = _operationCount)]
    public void Serilog()
    {
        for (var i = 0; i < _operationCount; ++i)
            _serilogLogger.Information("Hi {name}! It's {hour:HH:mm:ss}, and the message is #{number}", _text, _date, _number);
    }
}
