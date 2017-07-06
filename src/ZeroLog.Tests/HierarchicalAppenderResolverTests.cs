using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Moq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests
{
    [TestFixture]
    class HierarchicalAppenderResolverTests
    {
        private HierarchicalResolver _resolver;
        private NamedAppender _appenderA;
        private NamedAppender _appenderB;
        private NamedAppender _appenderC;

        [SetUp]
        public void SetUp()
        {
            _resolver = new HierarchicalResolver();
            _appenderA = new NamedAppender(new Mock<IAppender>().Object, "A");
            _appenderB = new NamedAppender(new Mock<IAppender>().Object, "B");
            _appenderC = new NamedAppender(new Mock<IAppender>().Object, "C");
        }

        [Test]
        public void should_resolve_nothing()
        {
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("test");

            Check.That(appenders.IsNullOrEmpty());
        }

        [Test]
        public void should_resolve_root()
        {
            _resolver.AddNode("", new List<NamedAppender>{ _appenderA }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("test");

            Check.That(appenders.Contains(_appenderA.Appender));
        }

        [Test]
        public void should_resolve_child_node()
        {
            _resolver.AddNode("Abc.Zebus", new List<NamedAppender>(), Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<NamedAppender> { _appenderA }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Appender));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_resolve_child_and_parent_node(bool includeParents)
        {
            _resolver.AddNode("Abc.Zebus", new List<NamedAppender> { _appenderB }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<NamedAppender> { _appenderA }, Level.Error, includeParents, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc", new List<NamedAppender> { _appenderC }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Appender));
            Check.That(appenders.Contains(_appenderB.Appender) == includeParents);
            Check.That(appenders.Contains(_appenderC.Appender) == false);
        }

        [Test]
        public void should_not_have_the_same_appender_twice()
        {
            _resolver.AddNode("Abc.Zebus", new List<NamedAppender> { _appenderA }, Level.Info, true, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc", new List<NamedAppender> { _appenderA, _appenderB }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Count).Equals(2);
        }
    }
}
