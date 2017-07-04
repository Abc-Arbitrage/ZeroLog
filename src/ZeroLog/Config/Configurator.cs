using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jil;
using ZeroLog.Appenders.Builders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Config
{
    public static class Configurator
    {
        public static (RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition, LogManagerConfiguration configuration) LoadFromJson(string jsonConfiguration)
        {
            var config = JSON.Deserialize<ZeroLogConfiguration>(jsonConfiguration);
            var legacyConfiguration = new LogManagerConfiguration
            {
                Level = config.Root.DefaultLevel,
                LogEventPoolExhaustionStrategy = config.Root.DefaultLogEventPoolExhaustionStrategy,
                LogEventBufferSize = config.Root.LogEventBufferSize,
                LogEventQueueSize = config.Root.LogEventQueueSize
            };

            return (config.Root, config.Loggers, config.Appenders, legacyConfiguration);
        }

        public static IConfigurationResolver BuildResolver(IAppenderFactory factory, RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition)
        {
            var hierarchicalResolver = new HierarchicalResolver();

            var appenders = appendersDefinition.ToDictionary(x => x.Name, factory.BuildAppender);

            hierarchicalResolver.AddNode("", rootDefinition.AppenderReferences.Select(x => appenders[x]), rootDefinition.DefaultLevel, false, rootDefinition.DefaultLogEventPoolExhaustionStrategy);

            foreach (var loggerDefinition in loggersDefinition)
            {
                hierarchicalResolver.AddNode(loggerDefinition.Name, loggerDefinition.AppenderReferences.Select(x => appenders[x]), loggerDefinition.Level, loggerDefinition.IncludeParentAppenders, loggerDefinition.LogEventPoolExhaustionStrategy);
            }

            hierarchicalResolver.Build();
            return hierarchicalResolver;
        }

        public static ILogManager Configure(IAppenderFactory factory, string filepath)
        {
            var filecontent = File.ReadAllText(filepath);
            var (r, l, a, c) = LoadFromJson(filecontent);
            return LogManager.Initialize(BuildResolver(factory, r, l, a), c);
        }
    }
}
