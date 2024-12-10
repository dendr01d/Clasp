using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    /// <summary>
    /// Maps <see cref="ScopeSet"/> instances to Names (strings)
    /// </summary>
    internal class ScopeMap
    {
        private List<KeyValuePair<ScopeSet, string>> _map;

        public ScopeMap()
        {
            _map = new List<KeyValuePair<ScopeSet, string>>();
        }

        public void AddMapping(ScopeSet set, string name)
        {
            _map.Add(new KeyValuePair<ScopeSet, string>(set, name));
        }

        /// <summary>
        /// Find the mapped scopes containing the maximal number of elements of <paramref name="set"/>.
        /// </summary>
        /// <returns>The subset size, and the matched mappings, if any.</returns>
        public KeyValuePair<ScopeSet, string>[] LookupLargestSubset(ScopeSet set)
        {
            if (set.ScopeSize == 0)
            {
                return Array.Empty<KeyValuePair<ScopeSet, string>>();
            }

            //this is slow, but binding resolution should only happen infrequently
            var subsetRankings = _map
                .Select(x => new Tuple<int, KeyValuePair<ScopeSet, string>>(x.Key.SubsetSize(set), x));

            if (!subsetRankings.Any())
            {
                return Array.Empty<KeyValuePair<ScopeSet, string>>();
            }
            else
            {
                int maxRank = subsetRankings.Max(x => x.Item1);

                return subsetRankings
                    .Where(x => x.Item1 == maxRank)
                    .Select(x => x.Item2)
                    .ToArray();
            }
        }
    }
}
