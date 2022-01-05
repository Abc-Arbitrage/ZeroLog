using System;
using System.IO;
using System.Threading;
using ZeroLog.ConfigResolvers;
using ZeroLog.Utils;

namespace ZeroLog.Config
{
    public static class JsonConfigurator
    {
        public static ILogManager ConfigureAndWatch(string configFilePath)
        {
            var configFileFullPath = Path.GetFullPath(configFilePath);

            var resolver = new HierarchicalResolver();

            var config = ConfigureResolver(configFileFullPath, resolver);

            var watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(configFileFullPath)!,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            watcher.Changed += (sender, args) =>
            {
                try
                {
                    if (!string.Equals(args.FullPath, configFileFullPath, StringComparison.InvariantCultureIgnoreCase))
                        return;

                    var newConfig = ReadConfiguration(configFileFullPath);
                    resolver.Build(newConfig);
                    ConfigureGlobal(newConfig);
                }
                catch (Exception e)
                {
                    LogManager.GetLogger(typeof(JsonConfigurator))
                              .Fatal($"Updating config failed with: ", e);
                }
            };

            var logManager = LogManager.Initialize(resolver, config.GetInitializationConfig());
            ConfigureGlobal(config);
            return logManager;
        }

        private static void ConfigureGlobal(ZeroLogJsonConfiguration config)
        {
            LogManager.Config.LazyRegisterEnums = config.LazyRegisterEnums;

            if (config.NullDisplayString != null)
                LogManager.Config.NullDisplayString = config.NullDisplayString;

            if (config.JsonSeparator != null)
                LogManager.Config.JsonSeparator = config.JsonSeparator;
        }

        private static ZeroLogJsonConfiguration ConfigureResolver(string configFileFullPath, HierarchicalResolver resolver)
        {
            var config = ReadConfiguration(configFileFullPath);
            resolver.Build(config);
            return config;
        }

        private static ZeroLogJsonConfiguration ReadConfiguration(string configFilePath)
        {
            var filecontent = ReadFileContentWithRetry(configFilePath);
            return DeserializeConfiguration(filecontent);
        }

        internal static ZeroLogJsonConfiguration DeserializeConfiguration(string? jsonConfiguration)
        {
            var config = JsonExtensions.DeserializeOrDefault(jsonConfiguration, new ZeroLogJsonConfiguration());
            return config;
        }

        private static string? ReadFileContentWithRetry(string filepath)
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
