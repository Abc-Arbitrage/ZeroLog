using Jil;
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
        public void resolve_appender()
        {
            var appenderDef = new AppenderDefinition
            {
                Name = "ExtApp1",
                AppenderTypeName = "ZeroLog.Tests.ExternalAppender.TestAppender,ZeroLog.Tests.ExternalAppender",
                AppenderJsonConfig = new DefaultAppenderConfig { PrefixPattern = "[%level] @ %time - %logger: " }
            };
            var config = new ZeroLogJsonConfiguration
            {
                LogEventBufferSize = 5,
                LogEventQueueSize = 7,
                RootLogger = new LoggerDefinition
                {
                    Level = Level.Info,
                    LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessage,
                    AppenderReferences = new[] { "ExtApp1" },

                },
                Appenders = new[] { appenderDef },
            };
            var configResolver = new HierarchicalResolver();
            configResolver.Build(config);

            Assert.DoesNotThrow(() => LogManager.Initialize(configResolver));

            ILog logger = null;
            Assert.DoesNotThrow(() => logger = LogManager.GetLogger(typeof(ExternalAppenderTest)));
            Assert.DoesNotThrow(() => logger.Info("Logger initialized."));

            LogManager.Shutdown();
        }
    }
}
