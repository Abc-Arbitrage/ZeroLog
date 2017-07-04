using System.Linq;
using Jil;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Appenders.Builders;
using ZeroLog.Config;

namespace ZeroLog.Tests.Config
{
    [TestFixture]
    public class ConfiguratorTests
    {

        [Test]
        public void should_load_configuration()
        {
            var appenderA = new AppenderDefinition { Name = "A", AppenderTypeName = nameof(ConsoleAppender), AppenderJsonConfig = JSON.Serialize(new ConsoleAppenderBuilder.Config{PrefixPattern = "[%level] @ %time - %logger: " })};
            var appenderB = new AppenderDefinition { Name = "B", AppenderTypeName = nameof(DateAndSizeRollingFileAppender), AppenderJsonConfig = JSON.Serialize(new DateAndSizeRollingFileAppenderBuilder.Config { FilepathRoot = "totopath " }) };
            var config = new ZeroLogConfiguration
            {
                Root = new RootDefinition
                {
                    DefaultLevel = Level.Warn,
                    DefaultLogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessage,
                    AppenderReferences = new []{ "A" },
                    LogEventBufferSize = 5,
                    LogEventQueueSize = 7
                },
                Appenders = new[] { appenderA, appenderB },
                Loggers = new[] {new LoggerDefinition{ Name = "Abc.Zebus", Level = Level.Debug, AppenderReferences = new []{ "B" } }}
            };
            var configJson = JSON.Serialize(config, Options.PrettyPrint);


            var (root, loggers, appenders, _) = Configurator.LoadFromJson(configJson);

            Check.That(root.DefaultLevel).Equals(config.Root.DefaultLevel);
            Check.That(root.LogEventBufferSize).Equals(config.Root.LogEventBufferSize);
            Check.That(root.LogEventQueueSize).Equals(config.Root.LogEventQueueSize);
            Check.That(root.AppenderReferences.Single()).Equals(config.Root.AppenderReferences.Single());

            Check.That(appenders.Single(a => a.Name == "A").Name).Equals(appenderA.Name);
            Check.That(appenders.Single(a => a.Name == "A").AppenderTypeName).Equals(appenderA.AppenderTypeName);
            Check.That(appenders.Single(a => a.Name == "A").AppenderJsonConfig).Equals(appenderA.AppenderJsonConfig);

            Check.That(appenders.Single(a => a.Name == "B").Name).Equals(appenderB.Name);
            Check.That(appenders.Single(a => a.Name == "B").AppenderTypeName).Equals(appenderB.AppenderTypeName);
            Check.That(appenders.Single(a => a.Name == "B").AppenderJsonConfig).Equals(appenderB.AppenderJsonConfig);
        }

        [Test]
        public void should_handle_missing_part()
        {
            var configJson = @"{
                                    ""Root"": {
                                    ""AppenderReferences"": [
                                        ""A""
                                    ],
                                    ""DefaultLevel"": ""Warn""
                                    },
                                    ""Appenders"": [
                                    {
                                        ""Name"": ""A"",
                                        ""AppenderTypeName"": ""ConsoleAppender""
                                    },
                                    {
                                        ""Name"": ""B"",
                                        ""AppenderTypeName"": ""DateAndSizeRollingFileAppender"",
                                        ""AppenderJsonConfig"": ""{\""FilepathRoot\"":\""totopath \""}""
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

            var (root, loggers, appenders, _) = Configurator.LoadFromJson(configJson);

            Check.That(root.DefaultLogEventPoolExhaustionStrategy).Equals(LogEventPoolExhaustionStrategy.Default);
            Check.That(root.LogEventBufferSize).Equals(10);
            Check.That(root.LogEventQueueSize).Equals(10);
        }
    }
}
