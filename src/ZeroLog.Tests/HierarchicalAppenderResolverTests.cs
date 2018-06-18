using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests
{
    [TestFixture]
    internal class HierarchicalAppenderResolverTests
    {
        private HierarchicalResolver _resolver;
        private ZeroLogConfiguration _config;

        public class TestAppenderParameters
        {
            public string Name { get; set; }

            public TestAppenderParameters(string name) => Name = name;
        }

        [SetUp]
        public void SetUp()
        {
            _resolver = new HierarchicalResolver();

            _config = new ZeroLogConfiguration();
            _config.RootLogger = new LoggerDefinition(string.Empty, Level.Info, false, LogEventPoolExhaustionStrategy.Default, "A");

            _config.Appenders = new[]
            {
                new AppenderDefinition {Name = "A", AppenderTypeName = typeof(TestAppender).FullName},
                new AppenderDefinition {Name = "B", AppenderTypeName = typeof(TestAppender).FullName},
                new AppenderDefinition {Name = "C", AppenderTypeName = typeof(TestAppender).FullName}
            };
        }

        [Test]
        public void should_resolve_root()
        {
            _resolver.Build(_config);

            var appenders = _resolver.ResolveAppenders("test");
            
            Check.That(appenders.Single().Name == "A");
        }

        [Test]
        public void should_resolve_child_node()
        {
            _config.Loggers = new[]
            {
                new LoggerDefinition("Abc.Zebus", Level.Info, false, LogEventPoolExhaustionStrategy.Default),
                new LoggerDefinition("Abc.Zebus.Dispatch", Level.Info, false, LogEventPoolExhaustionStrategy.Default, "A")
            };

            _resolver.Build(_config);

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Single().Name == "A").IsTrue();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_resolve_child_and_parent_node(bool includeParents)
        {
            _config.Loggers = new[]
            {
                new LoggerDefinition("Abc.Zebus", Level.Info, false, LogEventPoolExhaustionStrategy.Default, "B"),
                new LoggerDefinition("Abc.Zebus.Dispatch", Level.Error, includeParents, LogEventPoolExhaustionStrategy.Default, "A"),
                new LoggerDefinition("Abc", Level.Info, false, LogEventPoolExhaustionStrategy.Default, "C")
            };

            _resolver.Build(_config);

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Any(x => x.Name == "A")).IsTrue();
            Check.That(appenders.Any(x => x.Name == "B")).Equals(includeParents);
            Check.That(appenders.Any(x => x.Name == "C")).IsFalse();
        }

        [Test]
        public void should_not_have_the_same_appender_twice()
        {
            _config.Loggers = new[]
            {
                new LoggerDefinition("Abc.Zebus", Level.Info, true, LogEventPoolExhaustionStrategy.Default, "A"),
                new LoggerDefinition("Abc", Level.Info, false, LogEventPoolExhaustionStrategy.Default, "A", "B")
            };

            _resolver.Build(_config);

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Length).Equals(2);
        }
    }
}
