using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using ZeroLog.Appenders;
using ZeroLog.Benchmarks.Tools;
using ZeroLog.Configuration;

namespace ZeroLog.Benchmarks.CrossVersion;

#nullable enable

[Config(typeof(Config))]
public class CrossVersionBenchmarks
{
    private const int _iterationsPerInvoke = 256 * 1024;

    private static readonly Log _logger = LogManager.GetLogger("Log");

    private static int Answer => 42;
    private static AnswerEnum EnumAnswer => AnswerEnum.FortyTwo;

    [GlobalSetup]
    public void GlobalSetup()
    {
        LogManager.RegisterEnum<AnswerEnum>();

        LogManager.Initialize(new ZeroLogConfiguration
        {
            LogMessagePoolSize = _iterationsPerInvoke * 4,
            RootLogger =
            {
                Level = LogLevel.Debug,
                LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.Default,
                Appenders = { new NoopAppender() }
            }
        });
    }

    [IterationCleanup]
    public void IterationCleanup()
        => LogManager.Flush();

    [GlobalCleanup]
    public void GlobalCleanup()
        => LogManager.Shutdown();

    [Benchmark(OperationsPerInvoke = _iterationsPerInvoke)]
    public void LogChain()
    {
        for (var i = 0; i < _iterationsPerInvoke; ++i)
        {
            _logger.Info()
                   .Append("The answer is ")
                   .Append(Answer)
                   .Append(", which means ")
                   .AppendEnum(EnumAnswer)
                   .Append(".")
                   .AppendKeyValue("Answer", Answer)
                   .AppendKeyValue("NullableAnswer", (int?)Answer)
                   .AppendKeyValue("EnumAnswer", EnumAnswer)
                   .AppendKeyValue("NullableEnumAnswer", (AnswerEnum?)EnumAnswer)
                   .Log();
        }
    }

    private class Config : ManualConfig
    {
        public Config()
        {
            var nuGetVersion = NuGetTools.GetNuGetVersion();

            // Options |= ConfigOptions.KeepBenchmarkFiles;

            var jobConfig = Job.Default;

            if (AffinityHelper.GetPhysicalPCoreMask() is { } affinity)
                jobConfig = jobConfig.WithAffinity(affinity);

            AddJob(
                jobConfig.WithId("Current-Gen")
                         .WithMsBuildArguments("-p:ZeroLogSourceGenerators=true")
            );

            AddJob(
                jobConfig.WithId("Current-NoGen")
                         .WithMsBuildArguments("-p:ZeroLogSourceGenerators=false")
            );

            AddJob(
                jobConfig.WithId($"Release-v{nuGetVersion}")
                         .WithMsBuildArguments($"-p:ZeroLogNuGetVersion={nuGetVersion}")
                         .AsBaseline()
            );

            AddColumn(
                StatisticColumn.Median,
                StatisticColumn.OperationsPerSecond
            );
        }
    }

    internal enum AnswerEnum
    {
        None,
        FortyTwo = 42
    }
}
