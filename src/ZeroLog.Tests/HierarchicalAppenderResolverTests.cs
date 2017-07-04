using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Moq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;

namespace ZeroLog.Tests
{
    [TestFixture]
    class HierarchicalAppenderResolverTests
    {
        private HierarchicalAppenderResolver _resolver;
        private Mock<IAppender> _appenderA;
        private Mock<IAppender> _appenderB;
        private Mock<IAppender> _appenderC;

        [SetUp]
        public void SetUp()
        {
            _resolver = new HierarchicalAppenderResolver();
            _appenderA = new Mock<IAppender>();
            _appenderB = new Mock<IAppender>();
            _appenderC = new Mock<IAppender>();
        }

        [Test]
        public void should_resolve_nothing()
        {
            _resolver.Build();

            var appenders = _resolver.Resolve("test");

            Check.That(appenders.IsNullOrEmpty());
        }

        [Test]
        public void should_resolve_root()
        {
            _resolver.AddNode("", new List<IAppender>{ _appenderA.Object }, false);
            _resolver.Build();

            var appenders = _resolver.Resolve("test");

            Check.That(appenders.Contains(_appenderA.Object));
        }

        [Test]
        public void should_resolve_child_node()
        {
            _resolver.AddNode("Abc.Zebus", new List<IAppender>(), false);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<IAppender>{ _appenderA.Object }, false);
            _resolver.Build();

            var appenders = _resolver.Resolve("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Object));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void should_resolve_child_and_parent_node(bool includeParents)
        {
            _resolver.AddNode("Abc.Zebus", new List<IAppender>{ _appenderB.Object }, false);
            _resolver.AddNode("Abc.Zebus.Dispatch", new List<IAppender>{ _appenderA.Object }, includeParents);
            _resolver.AddNode("Abc", new List<IAppender>{ _appenderC.Object }, false);
            _resolver.Build();

            var appenders = _resolver.Resolve("Abc.Zebus.Dispatch.Handler");

            Check.That(appenders.Contains(_appenderA.Object));
            Check.That(appenders.Contains(_appenderB.Object) == includeParents);
            Check.That(appenders.Contains(_appenderC.Object) == false);
        }
    }
}
