﻿using System.Collections.Generic;
using Castle.Core.Internal;
using Moq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests
{
    [TestFixture]
    class HierarchicalAppenderResolverTests
    {
        private HierarchicalResolver _resolver;
        private Mock<IAppender> _appenderA;
        private Mock<IAppender> _appenderB;
        private Mock<IAppender> _appenderC;

        [SetUp]
        public void SetUp()
        {
            _resolver = new HierarchicalResolver();
            _appenderA = new Mock<IAppender>();
            _appenderB = new Mock<IAppender>();
            _appenderC = new Mock<IAppender>();
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
            _resolver.AddNode("", new List<IAppender>{ _appenderA.Object }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("test");

            Check.That(appenders.Contains(_appenderA.Object));
        }

        [Test]
        public void should_resolve_child_node()
        {
            _resolver.AddNode("Abc.Zebus", new List<IAppender>(), Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<IAppender>{ _appenderA.Object }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Object));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_resolve_child_and_parent_node(bool includeParents)
        {
            _resolver.AddNode("Abc.Zebus", new List<IAppender>{ _appenderB.Object }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<IAppender>{ _appenderA.Object }, Level.Error, includeParents, LogEventPoolExhaustionStrategy.Default);
            _resolver.AddNode("Abc", new List<IAppender>{ _appenderC.Object }, Level.Info, false, LogEventPoolExhaustionStrategy.Default);
            _resolver.Build();

            var appenders = _resolver.ResolveAppenders("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Object));
            Check.That(appenders.Contains(_appenderB.Object) == includeParents);
            Check.That(appenders.Contains(_appenderC.Object) == false);
        }
    }
}