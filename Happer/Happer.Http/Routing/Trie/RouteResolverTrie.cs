using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Happer.Http.Routing.Trie.Nodes;

namespace Happer.Http.Routing.Trie
{
    public class RouteResolverTrie
    {
        private readonly TrieNodeFactory _nodeFactory;
        private readonly IDictionary<string, TrieNode> _routeTries = new Dictionary<string, TrieNode>(StringComparer.OrdinalIgnoreCase);
        private static char[] _splitSeparators = new[] { '/' };

        public RouteResolverTrie(TrieNodeFactory nodeFactory)
        {
            this._nodeFactory = nodeFactory;
        }

        public void BuildTrie(RouteCache cache)
        {
            foreach (var cacheItem in cache)
            {
                var moduleKey = cacheItem.Key;
                var routeDefinitions = cacheItem.Value;

                foreach (var routeDefinition in routeDefinitions)
                {
                    var routeIndex = routeDefinition.Item1;
                    var routeDescription = routeDefinition.Item2;

                    TrieNode trieNode;
                    if (!this._routeTries.TryGetValue(routeDescription.Method, out trieNode))
                    {
                        trieNode = this._nodeFactory.GetNodeForSegment(null, null);

                        this._routeTries.Add(routeDefinition.Item2.Method, trieNode);
                    }

                    var segments = routeDefinition.Item2.Segments.ToArray();

                    trieNode.Add(segments, moduleKey, routeIndex, routeDescription);
                }
            }
        }

        public MatchResult[] GetMatches(string method, string path, Context context)
        {
            TrieNode result;

            if (!this._routeTries.TryGetValue(method, out result))
            {
                return MatchResult.NoMatches;
            }

            return result.GetMatches(path.Split(_splitSeparators, StringSplitOptions.RemoveEmptyEntries), context)
                         .ToArray();
        }

        public IEnumerable<string> GetOptions(string path, Context context)
        {
            foreach (var method in this._routeTries.Keys)
            {
                if (this.GetMatches(method, path, context).Any())
                {
                    yield return method;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var kvp in this._routeTries)
            {
                var method = kvp.Key;
                sb.Append(
                    kvp.Value
                    .GetRoutes()
                    .Select(s => method + " " + s)
                    .Aggregate((r1, r2) => r1 + "\n" + r2));
            }

            return sb.ToString();
        }
    }
}