using System.Linq;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests.Appenders
{
    [TestFixture]
    public class ExternalAppenderTest
    {
        [Test]
        public void should_resolve_appender_from_assembly_qualified_name()
        {
            var appenderDef = new AppenderDefinition
            {
                Name = "ExtApp1",
                AppenderTypeName = "ZeroLog.Tests.ExternalAppender.TestAppender, ZeroLog.Tests.ExternalAppender",
                AppenderJsonConfig = new DefaultAppenderConfig { PrefixPattern = "[%level] @ %time - %logger: " }
            };

            var config = new ZeroLogJsonConfiguration
            {
                LogMessageBufferSize = 5,
                LogMessagePoolSize = 7,
                RootLogger = new LoggerDefinition
                {
                    Level = Level.Info,
                    LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage,
                    AppenderReferences = new[] { "ExtApp1" },
                },
                Appenders = new[] { appenderDef },
            };

            var configResolver = new HierarchicalResolver();
            configResolver.Build(config);

            var appenders = configResolver.GetAllAppenders().ToList();
            Assert.AreEqual(1, appenders.Count);
            Assert.AreEqual("ZeroLog.Tests.ExternalAppender.TestAppender", ((GuardedAppender)appenders[0]).Appender.GetType().FullName);
        }
    }
}
