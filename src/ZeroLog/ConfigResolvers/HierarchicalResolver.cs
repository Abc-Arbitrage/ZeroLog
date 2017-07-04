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
        private class Node
        {
            public Dictionary<string, Node> Childrens = new Dictionary<string, Node>();
            public IEnumerable<IAppender> Appenders;
            public Level Level;
            public LogEventPoolExhaustionStrategy Strategy;
        }

        private struct Config
        {
            public string Name;
            public IEnumerable<IAppender> Appenders;
            public Level Level;
            public bool IncludeParentsAppenders;
            public LogEventPoolExhaustionStrategy Strategy;

            public Config(string name, IEnumerable<IAppender> appenders, Level level, bool includeParentsAppenders, LogEventPoolExhaustionStrategy strategy)
            {
                Name = name;
                Appenders = appenders;
                Level = level;
                Strategy = strategy;
                IncludeParentsAppenders = includeParentsAppenders;
            }
        }

        private Node _root;
        private readonly List<Config> _buildList = new List<Config>();
        private Encoding AppendersEncoding { get; set; }

        public void AddNode(string name, IEnumerable<IAppender> appenders, Level level, bool includeParentsAppenders, LogEventPoolExhaustionStrategy strategy)
        {
            var initializedAppenders = appenders?.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))) ?? Enumerable.Empty<IAppender>();
            if(AppendersEncoding != null)
                foreach (var appender in initializedAppenders)
                    appender.SetEncoding(AppendersEncoding);

            _buildList.Add(new Config (name, initializedAppenders, level, includeParentsAppenders, strategy));
        }

        public void Build()
        {
            void InternalAddNode(Config config)
            {
                var parts = config.Name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var node = _root;
                var path = "";

                foreach (var part in parts)
                {
                    path = (path + "." + part).Trim('.');

                    if (!node.Childrens.ContainsKey(part))
                        node.Childrens[part] = new Node { Appenders = node.Appenders, Level = node.Level, Strategy = node.Strategy };

                    node = node.Childrens[part];
                }

                node.Appenders = (config.IncludeParentsAppenders ? config.Appenders.Union(node.Appenders) : config.Appenders).Distinct();
                node.Strategy = config.Strategy;
                node.Level = config.Level;
            }

            _root = new Node();

            foreach (var item in _buildList.OrderBy(x => x.Name))
                InternalAddNode(item);

            _buildList.Clear();
            Updated();
        }

        private Node Resolve(string name)
        {
            var parts = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var node = _root;

            foreach (var part in parts)
            {
                if (node.Childrens.ContainsKey(part))
                    node = node.Childrens[part];
                else
                    break;
            }

            return node;
        }

        public IList<IAppender> ResolveAppenders(string name)
            => Resolve(name).Appenders?.ToList();
        public Level ResolveLevel(string name)
            => Resolve(name).Level;
        public LogEventPoolExhaustionStrategy ResolveExhaustionStrategy(string name)
            => Resolve(name).Strategy;

        public void Initialize(Encoding encoding)
        {
            AppendersEncoding = encoding;
            Initialize(_root);
        }

        private void Initialize(Node node)
        {
            node.Appenders = new List<IAppender>(node.Appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15))));

            foreach (var appender in node.Appenders)
                appender.SetEncoding(AppendersEncoding);

            foreach(var n in node.Childrens)
                Initialize(n.Value);
        }

        public event Action Updated = delegate {};
    }
}