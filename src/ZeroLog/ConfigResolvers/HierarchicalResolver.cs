using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;
using ZeroLog.Config;

namespace ZeroLog.ConfigResolvers
{
    public class HierarchicalResolver : IConfigurationResolver
    {
        private Node? _root;
        private Encoding AppenderEncoding { get; set; } = LogManager.DefaultEncoding;

        public IEnumerable<IAppender> GetAllAppenders()
        {
            var appenders = new HashSet<IAppender>();
            AddAppenders(_root);
            return appenders;

            void AddAppenders(Node? node)
            {
                if (node == null)
                    return;

                appenders.UnionWith(node.Appenders);
                foreach (var childNode in node.Children.Values)
                    AddAppenders(childNode);
            }
        }

        public LogConfig ResolveLogConfig(string name)
        {
            var node = Resolve(name);

            return new LogConfig
            {
                Appenders = node.Appenders.ToArray(),
                Level = node.Level,
                LogEventPoolExhaustionStrategy = node.LogEventPoolExhaustionStrategy,
                LogEventArgumentExhaustionStrategy = node.LogEventArgumentExhaustionStrategy
            };
        }

        public event Action? Updated;

        [Obsolete]
        public void Build(ZeroLogConfiguration config)
        {
            Build((IHierarchicalConfiguration)config);
        }

        public void Build(IHierarchicalConfiguration config)
        {
            var oldRoot = _root;
            var newRoot = new Node();

            foreach (var loggerWithAppenders in CreateLoggersWithAppenders(config).OrderBy(x => x.logger.Name))
            {
                AddNode(newRoot, loggerWithAppenders.logger, loggerWithAppenders.appenders);
            }

            ApplyEncodingToAllAppenders(newRoot);

            _root = newRoot;

            Updated?.Invoke();

            oldRoot?.Dispose();
        }

        private static List<(LoggerDefinition logger, IAppender[] appenders)> CreateLoggersWithAppenders(IHierarchicalConfiguration config)
        {
            var appendersByNames = config.Appenders.ToDictionary(x => x.Name, CreateAppender);

            var loggerWithAppenders = new List<(LoggerDefinition, IAppender[])>();

            config.RootLogger.Name = string.Empty;
            config.RootLogger.IncludeParentAppenders = false;

            loggerWithAppenders.Add((config.RootLogger, config.RootLogger.AppenderReferences?.Select(x => appendersByNames[x]).ToArray() ?? new IAppender[0]));

            foreach (var loggerDefinition in config.Loggers)
            {
                loggerWithAppenders.Add((loggerDefinition, loggerDefinition.AppenderReferences?.Select(x => appendersByNames[x]).ToArray() ?? new IAppender[0]));
            }

            return loggerWithAppenders;
        }

        private static IAppender CreateAppender(AppenderDefinition appenderDefinition)
        {
            var appender = AppenderFactory.CreateAppender(appenderDefinition);
            return new GuardedAppender(appender, TimeSpan.FromSeconds(15));
        }

        private static void AddNode(Node root, LoggerDefinition logger, IAppender[] appenders)
        {
            var parts = logger.Name?.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            var node = root;
            var path = "";

            foreach (var part in parts)
            {
                path = (path + "." + part).Trim('.');

                if (!node.Children.ContainsKey(part))
                {
                    node.Children[part] = new Node
                    {
                        Appenders = node.Appenders,
                        Level = node.Level,
                        LogEventPoolExhaustionStrategy = node.LogEventPoolExhaustionStrategy,
                        LogEventArgumentExhaustionStrategy = node.LogEventArgumentExhaustionStrategy
                    };
                }

                node = node.Children[part];
            }

            node.Appenders = (logger.IncludeParentAppenders ? appenders.Union(node.Appenders) : appenders).Distinct();
            node.LogEventPoolExhaustionStrategy = logger.LogEventPoolExhaustionStrategy;
            node.LogEventArgumentExhaustionStrategy = logger.LogEventArgumentExhaustionStrategy;
            node.Level = logger.Level;
        }

        private Node Resolve(string name)
        {
            var parts = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var node = _root ?? throw new InvalidOperationException("The configuration has not been built");

            foreach (var part in parts)
            {
                if (!node.Children.ContainsKey(part))
                    break;

                node = node.Children[part];
            }

            return node;
        }

        public void Initialize(Encoding encoding)
        {
            AppenderEncoding = encoding;

            ApplyEncodingToAllAppenders(_root);
        }

        private void ApplyEncodingToAllAppenders(Node? node)
        {
            if (node is null)
                return;

            if (AppenderEncoding != null && node.Appenders != null)
            {
                foreach (var appender in node.Appenders)
                {
                    appender.SetEncoding(AppenderEncoding);
                }
            }

            foreach (var n in node.Children)
            {
                ApplyEncodingToAllAppenders(n.Value);
            }
        }

        public void Dispose()
        {
            _root?.Dispose();
        }

        private class Node : IDisposable
        {
            public readonly Dictionary<string, Node> Children = new Dictionary<string, Node>();
            public IEnumerable<IAppender> Appenders = Enumerable.Empty<IAppender>();
            public Level Level;
            public LogEventPoolExhaustionStrategy LogEventPoolExhaustionStrategy;
            public LogEventArgumentExhaustionStrategy LogEventArgumentExhaustionStrategy;

            public void Dispose()
            {
                foreach (var appender in Appenders)
                    appender.Dispose();

                foreach (var child in Children.Values)
                    child.Dispose();
            }
        }
    }
}
