using System.Linq;
using Newtonsoft.Json;
using NFluent;
using NUnit.Framework;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog.Tests.Config
{
    [TestFixture]
    public class ConfiguratorTests
    {
        // TODO kill JSON config

//         [Test]
//         public void should_load_configuration()
//         {
//             var appenderAConfig = new DefaultAppenderConfig { PrefixPattern = "[%level] @ %time - %logger: " };
//             var appenderBConfig = new DateAndSizeRollingFileAppenderConfig { FilePathRoot = "totopath " };
//
//             var appenderA = new AppenderDefinition { Name = "A", AppenderTypeName = nameof(ConsoleAppender), AppenderJsonConfig = appenderAConfig };
//             var appenderB = new AppenderDefinition { Name = "B", AppenderTypeName = nameof(DateAndSizeRollingFileAppender), AppenderJsonConfig = appenderBConfig };
//
//             var config = new ZeroLogJsonConfiguration
//             {
//                 LogMessageBufferSize = 5,
//                 LogMessagePoolSize = 7,
//                 RootLogger = new LoggerDefinition
//                 {
//                     Level = Level.Warn,
//                     LogMessagePoolExhaustionStrategy = LogMessagePoolExhaustionStrategy.DropLogMessage,
//                     AppenderReferences = new[] { "A" },
//                 },
//                 Appenders = new[] { appenderA, appenderB },
//                 Loggers = new[] { new LoggerDefinition { Name = "Abc.Zebus", Level = Level.Debug, AppenderReferences = new[] { "B" } } }
//             };
//
//             var configJson = JsonConvert.SerializeObject(config);
//
//             var loadedConfig = JsonConfigurator.DeserializeConfiguration(configJson);
//
//             Check.That(loadedConfig.LogMessageBufferSize).Equals(config.LogMessageBufferSize);
//             Check.That(loadedConfig.LogMessagePoolSize).Equals(config.LogMessagePoolSize);
//
//             Check.That(loadedConfig.RootLogger.Level).Equals(config.RootLogger.Level);
//             Check.That(loadedConfig.RootLogger.AppenderReferences.Single()).Equals(config.RootLogger.AppenderReferences.Single());
//
//             Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").Name).Equals(appenderA.Name);
//             Check.That(loadedConfig.Appenders.Single(a => a.Name == "A").AppenderTypeName).Equals(appenderA.AppenderTypeName);
//
//             Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").Name).Equals(appenderB.Name);
//             Check.That(loadedConfig.Appenders.Single(a => a.Name == "B").AppenderTypeName).Equals(appenderB.AppenderTypeName);
//
//             var appenderALoadedConfig = (DefaultAppenderConfig)AppenderFactory.GetAppenderParameters(loadedConfig.Appenders.Single(a => a.Name == "A"), typeof(DefaultAppenderConfig));
//             Check.That(appenderALoadedConfig.PrefixPattern).IsEqualTo(appenderAConfig.PrefixPattern);
//
//             var appenderBLoadedConfig = (DateAndSizeRollingFileAppenderConfig)AppenderFactory.GetAppenderParameters(loadedConfig.Appenders.Single(a => a.Name == "B"), typeof(DateAndSizeRollingFileAppenderConfig));
//             Check.That(appenderBLoadedConfig.Extension).IsEqualTo(appenderBConfig.Extension);
//             Check.That(appenderBLoadedConfig.PrefixPattern).IsEqualTo(appenderBConfig.PrefixPattern);
//             Check.That(appenderBLoadedConfig.FilePathRoot).IsEqualTo(appenderBConfig.FilePathRoot);
//             Check.That(appenderBLoadedConfig.MaxFileSizeInBytes).IsEqualTo(appenderBConfig.MaxFileSizeInBytes);
//         }
//
//         [Test]
//         public void should_handle_missing_part()
//         {
//             var configJson = @"{
//                                     ""RootLogger"": {
//                                     ""AppenderReferences"": [
//                                         ""A""
//                                     ],
//                                     ""Level"": ""Warn""
//                                     },
//
//                                     ""Appenders"": [
//                                     {
//                                         ""Name"": ""A"",
//                                         ""AppenderTypeName"": ""ConsoleAppender""
//                                     },
//                                     {
//                                         ""Name"": ""B"",
//                                         ""AppenderTypeName"": ""DateAndSizeRollingFileAppender"",
//                                         ""AppenderJsonConfig"": {""FilePathRoot"":""totopath ""}
//                                     }
//                                     ],
//
//                                     ""Loggers"": [{
//                                         ""Name"": ""Abc.Zebus"",
//                                         ""AppenderReferences"": [
//                                             ""B""
//                                         ],
//                                         ""Level"": ""Debug"",
//                                         ""LogMessagePoolExhaustionStrategy"": ""DropLogMessageAndNotifyAppenders""
//                                     }]
//                                }";
//
//             var config = JsonConfigurator.DeserializeConfiguration(configJson);
//
//             Check.That(config.RootLogger.LogMessagePoolExhaustionStrategy).Equals(LogMessagePoolExhaustionStrategy.Default);
//             Check.That(config.LogMessageBufferSize).Equals(new ZeroLogJsonConfiguration().LogMessageBufferSize);
//             Check.That(config.LogMessagePoolSize).Equals(new ZeroLogJsonConfiguration().LogMessagePoolSize);
//
//             var appenderConfig = (DateAndSizeRollingFileAppenderConfig)AppenderFactory.GetAppenderParameters(config.Appenders[1], typeof(DateAndSizeRollingFileAppenderConfig));
//             Check.That(appenderConfig.FilePathRoot).IsEqualTo("totopath ");
//         }
    }
}
