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
            public IEnumerable<NamedAppender> Appenders;
            public Level Level;
            public LogEventPoolExhaustionStrategy Strategy;
        }

        private struct Config
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

        private Node _root;
        public int LogEventQueueSize { get; set; }
        public int LogEventBufferSize { get; set; }
        private readonly List<Config> _buildList = new List<Config>();
        private Encoding AppendersEncoding { get; set; }

        public void AddNode(string name, IEnumerable<NamedAppender> appenders, Level level, bool includeParentsAppenders, LogEventPoolExhaustionStrategy strategy)
        {
            _buildList.Add(new Config (name, appenders, level, includeParentsAppenders, strategy));
        }

        public void Build()
        {
            void InternalAddNode(Node root, Config config)
            {
                var parts = config.Name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var node = root;
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

            var newRoot = new Node();
            var oldRoot = _root;

            foreach (var item in _buildList.OrderBy(x => x.Name))
                InternalAddNode(newRoot, item);

            Initialize(newRoot);

            _root = newRoot;
            _buildList.Clear();
            
            Updated();
            Close(oldRoot);
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
            => Resolve(name).Appenders?.Select(x => x.Appender).ToList();
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
            if(node.Appenders != null)
                node.Appenders = node.Appenders.Select(x => new NamedAppender(new GuardedAppender(x.Appender, TimeSpan.FromSeconds(15)), x.Name)).ToList();

            if(AppendersEncoding != null)
                foreach (var appender in node.Appenders)
                    appender.Appender.SetEncoding(AppendersEncoding);

            foreach(var n in node.Childrens)
                Initialize(n.Value);
        }

        private void Close(Node node)
        {
            if (node == null)
                return;

            foreach (var appender in node.Appenders)
                appender.Appender.Close();

            foreach(var n in node.Childrens)
                Close(n.Value);
        }

        public event Action Updated = delegate {};

        public void Dispose()
        {
            Close(_root);
        }
    }
}