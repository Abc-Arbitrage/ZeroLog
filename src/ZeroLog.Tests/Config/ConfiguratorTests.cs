using System.Linq;
using Jil;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Tests.Config
{
    [TestFixture]
    public class ConfiguratorTests
    {
        [Test]
        public void should_load_configuration()
        {
            var appenderA = new AppenderDefinition { Name = "A", AppenderTypeName = nameof(ConsoleAppender), AppenderJsonConfig = JSON.Serialize(new DefaultAppenderConfig{PrefixPattern = "[%level] @ %time - %logger: " })};
            var appenderB = new AppenderDefinition { Name = "B", AppenderTypeName = nameof(DateAndSizeRollingFileAppender), AppenderJsonConfig = JSON.Serialize(new DateAndSizeRollingFileAppenderConfig { FilePathRoot = "totopath " }) };
            var config = new ZeroLogConfiguration
            {
                LogEventBufferSize = 5,
                LogEventQueueSize = 7,
                RootLogger = new LoggerDefinition
                {
                    Level = Level.Warn,
                    LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessage,
                    AppenderReferences = new []{ "A" },
  
                },
                Appenders = new[] { appenderA, appenderB },
                Loggers = new[] {new LoggerDefinition{ Name = "Abc.Zebus", Level = Level.Debug, AppenderReferences = new []{ "B" } }}
            };
            var configJson = JSON.Serialize(config, Options.PrettyPrint);

            var loadedConfig = JsonConfigurator.DeserializeConfiguration(configJson);

            Check.That(loadedConfig.LogEventBufferSize).Equals(config.LogEventBufferSize);
            Check.That(loadedConfig.LogEventQueueSize).Equals(config.LogEventQueueSize);

            Check.That(loadedConfig.RootLogger.Level).Equals(config.RootLogger.Level);
            Check.That(loadedConfig.RootLogger.AppenderReferences.Single()).Equals(config.RootLogger.AppenderReferences.Single());

            Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").Name).Equals(appenderA.Name);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").AppenderTypeName).Equals(appenderA.AppenderTypeName);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").AppenderJsonConfig).Equals(appenderA.AppenderJsonConfig);

            Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").Name).Equals(appenderB.Name);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").AppenderTypeName).Equals(appenderB.AppenderTypeName);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").AppenderJsonConfig).Equals(appenderB.AppenderJsonConfig);
        }

        [Test]
        public void should_handle_missing_part()
        {
            var configJson = @"{
                                    ""RootLogger"": {
                                    ""AppenderReferences"": [
                                        ""A""
                                    ],
                                    ""Level"": ""Warn""
                                    },

                                    ""Appenders"": [
                                    {
                                        ""Name"": ""A"",
                                        ""AppenderTypeName"": ""ConsoleAppender""
                                    },
                                    {
                                        ""Name"": ""B"",
                                        ""AppenderTypeName"": ""DateAndSizeRollingFileAppender"",
                                        ""AppenderJsonConfig"": ""{\""FilePathRoot\"":\""totopath \""}""
                                    }
                                    ],

                                    ""Loggers"": [{
                                        ""Name"": ""Abc.Zebus"",
                                        ""AppenderReferences"": [
                                            ""B""
                                        ],
                                        ""Level"": ""Debug"",
                                        ""LogEventPoolExhaustionStrategy"": ""DropLogMessageAndNotifyAppenders""
                                    }]
                               }";

            var config = JsonConfigurator.DeserializeConfiguration(configJson);

            Check.That(config.RootLogger.LogEventPoolExhaustionStrategy).Equals(LogEventPoolExhaustionStrategy.Default);
            Check.That(config.LogEventBufferSize).Equals(10);
            Check.That(config.LogEventQueueSize).Equals(10);
        }
    }
}
