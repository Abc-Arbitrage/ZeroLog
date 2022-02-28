using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Benchmarks.Logging
{
    public class AppendingBenchmarks
    {
        private ILog _log;

        private readonly int _intField = 42;
        private readonly string _stringField = "string";
        private readonly double _doubleField = 42.42;
        private readonly DateTime _dateTimeField = new(2022, 02, 22);
        private BenchmarkLogManager _internalLogManager;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var configResolver = new BasicResolver(Enumerable.Empty<IAppender>(), Level.Finest, LogEventPoolExhaustionStrategy.Default);
            var config = new ZeroLogInitializationConfig();
            _internalLogManager = new BenchmarkLogManager(new LogManager(configResolver, config), config);
            _log = new ZeroLog.Log(_internalLogManager, "BenchmarkV1");
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _internalLogManager.Dispose();
        }

        [Benchmark]
        public void SimpleString()
        {
            _log.Debug("Lorem ipsum dolor sit amet");
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
}
