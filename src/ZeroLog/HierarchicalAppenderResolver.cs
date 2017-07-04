using System;
using System.Collections.Generic;
using System.Linq;
using ZeroLog.Appenders;

namespace ZeroLog
{
    public class HierarchicalAppenderResolver : IAppenderResolver
    {
        private class Node
        {
            public string Key;
            public Dictionary<string, Node> Childrens = new Dictionary<string, Node>();
            public IEnumerable<IAppender> Appenders;
            public bool IncludeParentsAppenders;
            public bool Real;
        }

        private Node _root;
        private List<Tuple<string, IEnumerable<IAppender>, bool>> _buildList = new List<Tuple<string, IEnumerable<IAppender>, bool>>();

        public void AddNode(string name, IEnumerable<IAppender> appenders, bool includeParentsAppenders)
        {
            _buildList.Add(new Tuple<string, IEnumerable<IAppender>, bool>(name, appenders, includeParentsAppenders));
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
                        node.Childrens[part] = new Node { Key = path, Appenders = node.Appenders };

                    node = node.Childrens[part];
                }

                node.Appenders = includeParentsAppenders ? appenders.Union(node.Appenders) : appenders;
                node.IncludeParentsAppenders = includeParentsAppenders;
                node.Real = true;
            }

            _root = new Node();

            foreach (var item in _buildList.OrderBy(x => x.Item1))
                InternalAddNode(item.Item1, item.Item2, item.Item3);
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
    }
}