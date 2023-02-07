using System;
using BenchmarkDotNet.Attributes;
using Serilog.Events;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;
using ZeroLog.Tests;

namespace ZeroLog.Benchmarks.LatencyTests;

[MemoryDiagnoser]
[AllStatisticsColumn]
[SimpleJob(iterationCount: 100, invocationCount: 8 * 1024)]
public class LatencyBenchmarks
{
    private static readonly string _text = "dude";
    private static readonly DateTime _date = DateTime.UtcNow;
    private static readonly int _number = 42;

    // ZeroLog
    private TestAppender _zeroLogTestAppender;
    private Log _zeroLogLogger;

    // Serilog
    private SerilogTestSink _serilogTestSink;
    private Serilog.Core.Logger _serilogLogger;

    [ParamsAllValues]
    public bool Enabled { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        SetupZeroLog();
        SetupSerilog();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        TearDownZeroLog();
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
            LogMessagePoolSize = 32 * 1024,
            RootLogger =
            {
                Level = Enabled ? LogLevel.Info : LogLevel.Warn,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.WaitUntilAvailable,
                Appenders = { _zeroLogTestAppender }
            }
        });

        _zeroLogLogger = LogManager.GetLogger(nameof(ZeroLog));
    }

    private void TearDownZeroLog()
        => LogManager.Shutdown();

    [Benchmark(Baseline = true)]
    public void ZeroLog()
        => _zeroLogLogger.Info($"Hi {_text} ! It's {_date:HH:mm:ss}, and the message is #{_number}");

    [IterationCleanup(Target = nameof(ZeroLog))]
    public void CleanupZeroLogIteration()
        => LogManager.WaitUntilQueueIsEmpty();

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
    }

    private void TearDownSerilog()
        => _serilogLogger.Dispose();

    [Benchmark]
    public void Serilog()
        => _serilogLogger.Information("Hi {name} ! It's {hour:HH:mm:ss}, and the message is #{number}", _text, _date, _number);
}
