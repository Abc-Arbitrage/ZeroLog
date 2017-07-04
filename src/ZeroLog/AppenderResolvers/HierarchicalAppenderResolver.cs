using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroLog.Appenders;

namespace ZeroLog.AppenderResolvers
{
    public class HierarchicalAppenderResolver : IAppenderResolver
    {
        private class Node
        {
            public Dictionary<string, Node> Childrens = new Dictionary<string, Node>();
            public IEnumerable<IAppender> Appenders;
        }

        private Node _root;
        private readonly List<Tuple<string, IEnumerable<IAppender>, bool>> _buildList = new List<Tuple<string, IEnumerable<IAppender>, bool>>();
        private Encoding AppendersEncoding { get; set; }

        public void AddNode(string name, IEnumerable<IAppender> appenders, bool includeParentsAppenders)
        {
            var initializedAppenders = appenders.Select(x => new GuardedAppender(x, TimeSpan.FromSeconds(15)));
            foreach(var appender in initializedAppenders)
                appender.SetEncoding(AppendersEncoding);

            _buildList.Add(new Tuple<string, IEnumerable<IAppender>, bool>(name, initializedAppenders, includeParentsAppenders));
        }

        public void Build()
        {
            void InternalAddNode(string name, IEnumerable<IAppender> appenders, bool includeParentsAppenders)
            {
                var parts = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var node = _root;
                var path = "";

                foreach (var part in parts)
                {
                    path = (path + "." + part).Trim('.');

                    if (!node.Childrens.ContainsKey(part))
                        node.Childrens[part] = new Node { Appenders = node.Appenders };

                    node = node.Childrens[part];
                }

                node.Appenders = includeParentsAppenders ? appenders.Union(node.Appenders) : appenders;
            }

            _root = new Node();

            foreach (var item in _buildList.OrderBy(x => x.Item1))
                InternalAddNode(item.Item1, item.Item2, item.Item3);

            _buildList.Clear();
        }
        
        public IList<IAppender> Resolve(string name)
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

            return node.Appenders?.ToList();
        }

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
    }
}