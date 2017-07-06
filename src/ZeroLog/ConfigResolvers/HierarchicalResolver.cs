using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.ConfigResolvers
{
    public class HierarchicalResolver : IConfigurationResolver
    {
        private Node _root;
        private readonly List<Config> _configs = new List<Config>();
        private Encoding AppenderEncoding { get; set; }

        public void AddNode(string name, IEnumerable<NamedAppender> appenders, Level level, bool includeParentsAppenders, LogEventPoolExhaustionStrategy strategy)
        {
            _configs.Add(new Config (name, appenders, level, includeParentsAppenders, strategy));
        }

        public void Build()
        {
            var newRoot = new Node();

            var oldRoot = _root;

            foreach (var item in _configs.OrderBy(x => x.Name))
            {
                InternalAddNode(newRoot, item);
            }

            Initialize(newRoot);

            _root = newRoot;
            _configs.Clear();
            
            Updated();

            oldRoot?.Close();
        }

        private static void InternalAddNode(Node root, Config config)
        {
            var parts = config.Name.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            var node = root;
            var path = "";

            foreach (var part in parts)
            {
                path = (path + "." + part).Trim('.');

                if (!node.Children.ContainsKey(part))
                    node.Children[part] = new Node {Appenders = node.Appenders, Level = node.Level, Strategy = node.Strategy};

                node = node.Children[part];
            }

            node.Appenders = (config.IncludeParentsAppenders ? config.Appenders.Union(node.Appenders) : config.Appenders).Distinct();
            node.Strategy = config.Strategy;
            node.Level = config.Level;
        }

        private Node Resolve(string name)
        {
            var parts = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var node = _root;

            foreach (var part in parts)
            {
                if (!node.Children.ContainsKey(part))
                    break;

                node = node.Children[part];
            }

            return node;
        }

        public IList<IAppender> ResolveAppenders(string name) => Resolve(name).Appenders?.Select(x => x.Appender).ToList();
        public Level ResolveLevel(string name) => Resolve(name).Level;
        public LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name) => Resolve(name).Strategy;

        public void Initialize(Encoding encoding)
        {
            AppenderEncoding = encoding;
            Initialize(_root);
        }

        private void Initialize(Node node)
        {
            if(node.Appenders != null)
                node.Appenders = node.Appenders.Select(x => new NamedAppender(new GuardedAppender(x.Appender, TimeSpan.FromSeconds(15)), x.Name)).ToList();

            if (AppenderEncoding != null)
            {
                foreach (var appender in node.Appenders)
                {
                    appender.Appender.SetEncoding(AppenderEncoding);
                }
            }

            foreach (var n in node.Children)
            {
                Initialize(n.Value);
            }
        }

        public event Action Updated = delegate {};

        public void Dispose()
        {
            _root?.Close();
        }

        private class Node
        {
            public readonly Dictionary<string, Node> Children = new Dictionary<string, Node>();
            public IEnumerable<NamedAppender> Appenders;
            public Level Level;
            public LogEventPoolExhaustionStrategy Strategy;

            public void Close()
            {
                foreach (var appender in Appenders)
                {
                    appender.Appender.Close();
                }

                foreach (var child in Children.Values)
                {
                    child.Close();
                }
            }
        }

        private class Config
        {
            public readonly string Name;
            public readonly IEnumerable<NamedAppender> Appenders;
            public readonly Level Level;
            public readonly bool IncludeParentsAppenders;
            public readonly LogEventPoolExhaustionStrategy Strategy;

            public Config(string name, IEnumerable<NamedAppender> appenders, Level level, bool includeParentsAppenders, LogEventPoolExhaustionStrategy strategy)
            {
                Name = name;
                Appenders = appenders;
                Level = level;
                Strategy = strategy;
                IncludeParentsAppenders = includeParentsAppenders;
            }
        }
    }
}