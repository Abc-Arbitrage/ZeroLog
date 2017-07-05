using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZeroLog.Appenders.Builders;
using ZeroLog.Config;
using ZeroLog.Utils;

namespace ZeroLog.ConfigResolvers
{
    public static class Configurator
    {
        public static ILogManager ConfigureAndWatch(string filepath)
        {
            var fullpath = Path.GetFullPath(filepath);
            var filecontent = File.Exists(filepath) ? File.ReadAllText(filepath) : "";
            var (r, l, a, c) = LoadFromJson(filecontent);
            var resolver = new HierarchicalResolver();

            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(fullpath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };
            watcher.Changed += (sender, args) =>
            {
                if (string.Equals(args.FullPath, fullpath, StringComparison.InvariantCultureIgnoreCase))
                    ConfigureResolver(filepath, resolver);
            };

            FillResolver(resolver, r, l, a);
            return LogManager.Initialize(resolver, c);
        }

        public static (RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition, LogManagerConfiguration configuration) LoadFromJson(string jsonConfiguration)
        {
            var config = JsonExtensions.DeserializeOrDefault(jsonConfiguration, new ZeroLogConfiguration());

            var legacyConfiguration = new LogManagerConfiguration
            {
                Level = config.Root.DefaultLevel,
                LogEventPoolExhaustionStrategy = config.Root.DefaultLogEventPoolExhaustionStrategy,
                LogEventBufferSize = config.Root.LogEventBufferSize,
                LogEventQueueSize = config.Root.LogEventQueueSize
            };

            return (config.Root, config.Loggers, config.Appenders, legacyConfiguration);
        }

        private static void FillResolver(HierarchicalResolver hierarchicalResolver, RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition)
        {
            var appenders = appendersDefinition.ToDictionary(x => x.Name, x => new NamedAppender(AppenderFactory.BuildAppender(x), x.Name));

            hierarchicalResolver.AddNode("", rootDefinition.AppenderReferences.Select(x => appenders[x]), rootDefinition.DefaultLevel, false, rootDefinition.DefaultLogEventPoolExhaustionStrategy);

            foreach (var loggerDefinition in loggersDefinition)
            {
                hierarchicalResolver.AddNode(loggerDefinition.Name, loggerDefinition.AppenderReferences.Select(x => appenders[x]), loggerDefinition.Level, loggerDefinition.IncludeParentAppenders, loggerDefinition.LogEventPoolExhaustionStrategy);
            }

            hierarchicalResolver.Build();
        }
        
        private static void ConfigureResolver(string filepath, HierarchicalResolver resolver)
        {
            var filecontent = SafeRead(filepath);
            var (r, l, a, c) = LoadFromJson(filecontent);

            FillResolver(resolver, r, l, a);
        }

        private static string SafeRead(string filepath)
        {
            const int numberOfRetries = 3;
            const int delayOnRetry = 1000;

            for (var i = 0; i < numberOfRetries; i++)
            {
                try
                {
                    return File.ReadAllText(filepath);
                }
                catch (IOException)
                {
                    Thread.Sleep(delayOnRetry);
                }
            }

            return null;
        }
    }
}
