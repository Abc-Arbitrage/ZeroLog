using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;
using ZeroLog.Appenders.Builders;
using ZeroLog.ConfigResolvers;

namespace ZeroLog.Config
{
    public static class Configurator
    {
        static IConfigurationResolver BuildResolver(IAppenderFactory factory, RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition)
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
    }
}
