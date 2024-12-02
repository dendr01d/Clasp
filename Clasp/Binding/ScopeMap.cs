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
        public Tuple<int, KeyValuePair<ScopeSet, string>[]> LookupLargestSubset(ScopeSet set)
        {
            //this is slow, but binding resolution should only happen infrequently

            var subsetRankings = _map
                .Select(x => new Tuple<int, KeyValuePair<ScopeSet, string>>(x.Key.SubsetSize(set), x))
                .OrderByDescending(x => x.Item1);

            if (!subsetRankings.Any())
            {
                return new Tuple<int, KeyValuePair<ScopeSet, string>[]>(0, Array.Empty<KeyValuePair<ScopeSet, string>>());
            }
            else
            {
                int maxRank = subsetRankings.First().Item1;

                var maxRankedMappings = subsetRankings.TakeWhile(x => x.Item1 == maxRank);

                return new Tuple<int, KeyValuePair<ScopeSet, string>[]>(maxRank, maxRankedMappings.Select(x => x.Item2).ToArray());
            }
        }
    }
}
