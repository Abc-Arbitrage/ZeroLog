using System;
using BenchmarkDotNet.Attributes;
using ZeroLog.Configuration;

namespace ZeroLog.Benchmarks.Logging;

public class AppendingBenchmarks
{
    private BenchmarkLogMessageProvider _provider;
    private ZeroLog.Log _log;

    private readonly int _intField = 42;
    private readonly string _stringField = "string";
    private readonly double _doubleField = 42.42;
    private readonly DateTime _dateTimeField = new(2022, 02, 22);

    [GlobalSetup]
    public void GlobalSetup()
    {
        _provider = new BenchmarkLogMessageProvider();
        _log = new ZeroLog.Log("BenchmarkV2");
        _log.UpdateConfiguration(_provider, ResolvedLoggerConfiguration.SingleAppender(LogLevel.Trace));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _provider.Dispose();
        LogManager.Shutdown();
    }

    [Benchmark]
    public void SimpleString()
    {
        _log.Debug("Lorem ipsum dolor sit amet");
    }

    [Benchmark]
    public void InterpolatedString2()
    {
        _log.Debug($"Lorem ipsum {_stringField} dolor sit amet {_stringField} dolor sit amet {_stringField} dolor sit amet {_stringField}");
    }

    [Benchmark]
    public void InterpolatedString()
    {
        _log.Debug($"Lorem ipsum {_intField} dolor sit amet {_doubleField} dolor sit amet {_dateTimeField} dolor sit amet {_stringField}");
    }

    [Benchmark]
    public void AppendedString()
    {
        _log.Debug()
            .Append("Lorem ipsum ").Append(_intField)
            .Append(" dolor sit amet ").Append(_doubleField)
            .Append(" dolor sit amet ").Append(_dateTimeField)
            .Append(" dolor sit amet ").Append(_stringField)
            .Log();
    }

    [Benchmark]
    public void AppendedString2()
    {
        _log.Debug()
            .Append("Lorem ipsum ").Append(_stringField)
            .Append(" dolor sit amet ").Append(_stringField)
            .Append(" dolor sit amet ").Append(_stringField)
            .Append(" dolor sit amet ").Append(_stringField)
            .Log();
    }
}
