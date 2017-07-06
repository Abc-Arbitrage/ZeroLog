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
            var resolver = new HierarchicalResolver();
            var fullpath = Path.GetFullPath(filepath);

            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(fullpath),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            ConfigureResolver(fullpath, resolver);

            watcher.Changed += (sender, args) =>
            {
                try
                {
                    if (string.Equals(args.FullPath, fullpath, StringComparison.InvariantCultureIgnoreCase))
                        ConfigureResolver(fullpath, resolver);
                }
                catch(Exception e)
                {
                    LogManager.GetLogger(typeof(Configurator))
                              .FatalFormat("Updating configuration failed with: {0}", e.Message);
                }
            };

            return LogManager.Initialize(resolver);
        }

        public static (RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition) LoadFromJson(string jsonConfiguration)
        {
            var config = JsonExtensions.DeserializeOrDefault(jsonConfiguration, new ZeroLogConfiguration());
            return (config.Root, config.Loggers, config.Appenders);
        }

        private static void FillResolver(HierarchicalResolver hierarchicalResolver, RootDefinition rootDefinition, IList<LoggerDefinition> loggersDefinition, IList<AppenderDefinition> appendersDefinition)
        {
            var appenders = appendersDefinition.ToDictionary(x => x.Name, x => new NamedAppender(AppenderFactory.BuildAppender(x), x.Name));

            hierarchicalResolver.LogEventBufferSize = rootDefinition.LogEventBufferSize;
            hierarchicalResolver.LogEventQueueSize = rootDefinition.LogEventQueueSize;

            hierarchicalResolver.AddNode("", rootDefinition.AppenderReferences.Select(x => appenders[x]), rootDefinition.DefaultLevel, false, rootDefinition.DefaultLogEventPoolExhaustionStrategy);

            foreach (var loggerDefinition in loggersDefinition)
            {
                hierarchicalResolver.AddNode(loggerDefinition.Name, loggerDefinition.AppenderReferences.Select(x => appenders[x]), loggerDefinition.Level, loggerDefinition.IncludeParentAppenders, loggerDefinition.LogEventPoolExhaustionStrategy);
            }

            hierarchicalResolver.Build();
        }
        
        private static void ConfigureResolver(string fullpath, HierarchicalResolver resolver)
        {
            var filecontent = SafeRead(fullpath);
            var (r, l, a) = LoadFromJson(filecontent);

            FillResolver(resolver, r, l, a);
        }

        private static string SafeRead(string fullpath)
        {
            const int numberOfRetries = 3;
            const int delayOnRetry = 1000;

            for (var i = 0; i < numberOfRetries; i++)
            {
                try
                {
                    return File.ReadAllText(fullpath);
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
