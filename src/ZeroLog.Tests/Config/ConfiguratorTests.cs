using System.Linq;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog.Tests.Config
{
    [TestFixture]
    public class ConfiguratorTests
    {
        [Test]
        public void should_load_configuration()
        {
            var appenderAConfig = new DefaultAppenderConfig { PrefixPattern = "[%level] @ %time - %logger: " };
            var appenderBConfig = new DateAndSizeRollingFileAppenderConfig { FilePathRoot = "totopath " };

            var appenderA = new AppenderDefinition { Name = "A", AppenderTypeName = nameof(ConsoleAppender), AppenderJsonConfig = appenderAConfig };
            var appenderB = new AppenderDefinition { Name = "B", AppenderTypeName = nameof(DateAndSizeRollingFileAppender), AppenderJsonConfig = appenderBConfig };

            var config = new ZeroLogJsonConfiguration
            {
                LogEventBufferSize = 5,
                LogEventQueueSize = 7,
                RootLogger = new LoggerDefinition
                {
                    Level = Level.Warn,
                    LogEventPoolExhaustionStrategy = LogEventPoolExhaustionStrategy.DropLogMessage,
                    AppenderReferences = new[] { "A" },
                },
                Appenders = new[] { appenderA, appenderB },
                Loggers = new[] { new LoggerDefinition { Name = "Abc.Zebus", Level = Level.Debug, AppenderReferences = new[] { "B" } } }
            };

            var configJson = @"
            {
              ""LazyRegisterEnums"": false,
              ""LogEventArgumentCapacity"": 32,
              ""LogEventBufferSize"": 5,
              ""LogEventQueueSize"": 7,
              ""Appenders"": [{
                ""AppenderJsonConfig"": {
                  ""PrefixPattern"": ""[%level] @ %time - %logger: ""
               },
               ""AppenderTypeName"": ""ConsoleAppender"",
               ""Name"": ""A""
              }, {
                ""AppenderJsonConfig"": {
                   ""FilePathRoot"": ""totopath ""
               },
               ""AppenderTypeName"": ""DateAndSizeRollingFileAppender"",
               ""Name"": ""B""
              }],
              ""Loggers"": [{
                ""IncludeParentAppenders"": false,
                ""AppenderReferences"": [""B""],
                ""Level"": ""Debug"",
                ""LogEventPoolExhaustionStrategy"": ""DropLogMessageAndNotifyAppenders"",
                ""LogEventArgumentExhaustionStrategy"": ""TruncateMessage"",
                ""Name"": ""Abc.Zebus""
              }],
              ""RootLogger"": {
                ""IncludeParentAppenders"": false,
                ""AppenderReferences"": [""A""],
                ""Level"": ""Warn"",
                ""LogEventPoolExhaustionStrategy"": ""DropLogMessage"",
                ""LogEventArgumentExhaustionStrategy"": ""TruncateMessage"",
                ""Name"": null
              },
              ""NullDisplayString"": null
            }";

            var loadedConfig = JsonConfigurator.DeserializeConfiguration(configJson);

            Check.That(loadedConfig.LogEventBufferSize).Equals(config.LogEventBufferSize);
            Check.That(loadedConfig.LogEventQueueSize).Equals(config.LogEventQueueSize);

            Check.That(loadedConfig.RootLogger.Level).Equals(config.RootLogger.Level);
            Check.That(loadedConfig.RootLogger.AppenderReferences.Single()).Equals(config.RootLogger.AppenderReferences.Single());

            Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").Name).Equals(appenderA.Name);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").AppenderTypeName).Equals(appenderA.AppenderTypeName);

            Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").Name).Equals(appenderB.Name);
            Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").AppenderTypeName).Equals(appenderB.AppenderTypeName);

            var appenderALoadedConfig = (DefaultAppenderConfig)AppenderFactory.GetAppenderParameters(loadedConfig.Appenders.Single(a => a.Name == "A"), typeof(DefaultAppenderConfig));
            Check.That(appenderALoadedConfig.PrefixPattern).IsEqualTo(appenderAConfig.PrefixPattern);

            var appenderBLoadedConfig = (DateAndSizeRollingFileAppenderConfig)AppenderFactory.GetAppenderParameters(loadedConfig.Appenders.Single(a => a.Name == "B"), typeof(DateAndSizeRollingFileAppenderConfig));
            Check.That(appenderBLoadedConfig.Extension).IsEqualTo(appenderBConfig.Extension);
            Check.That(appenderBLoadedConfig.PrefixPattern).IsEqualTo(appenderBConfig.PrefixPattern);
            Check.That(appenderBLoadedConfig.FilePathRoot).IsEqualTo(appenderBConfig.FilePathRoot);
            Check.That(appenderBLoadedConfig.MaxFileSizeInBytes).IsEqualTo(appenderBConfig.MaxFileSizeInBytes);
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
                                        ""AppenderJsonConfig"": {""FilePathRoot"":""totopath ""}
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
            Check.That(config.LogEventBufferSize).Equals(new ZeroLogJsonConfiguration().LogEventBufferSize);
            Check.That(config.LogEventQueueSize).Equals(new ZeroLogJsonConfiguration().LogEventQueueSize);

            var appenderConfig = (DateAndSizeRollingFileAppenderConfig)AppenderFactory.GetAppenderParameters(config.Appenders[1], typeof(DateAndSizeRollingFileAppenderConfig));
            Check.That(appenderConfig.FilePathRoot).IsEqualTo("totopath ");
        }
    }
}
