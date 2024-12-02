using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    internal class BindingStore
    {
        private Dictionary<string, ScopeMap> _bindingLookup;

        public BindingStore()
        {
            _bindingLookup = new Dictionary<string, ScopeMap>();
        }

        public void BindName(string newName, ScopeSet newSet, string boundName)
        {
            if (_bindingLookup.TryGetValue(newName, out ScopeMap? map))
            {
                map.AddMapping(newSet, boundName);
            }
            else
            {
                _bindingLookup[newName] = new ScopeMap();
                _bindingLookup[newName].AddMapping(newSet, boundName);
            }
        }

        public string ResolveName(string name, ScopeSet context)
        {
            if (_bindingLookup.TryGetValue(name, out ScopeMap? map))
            {
                Tuple<int, KeyValuePair<ScopeSet, string>[]> matches = map.LookupLargestSubset(context);

                if (matches.Item1 == 0)
                {
                    //throw new IdResolutionException(
                    //    "Name '{0}' with {1} {2} matched zero contexts in {3}.",
                    //    name,
                    //    nameof(ScopeSet),
                    //    context.ToString(),
                    //    nameof(BindingStore)
                    //    );
                    return name;
                }
                else if (matches.Item2.Length > 1)
                {
                    string formattedMatches = string.Join(System.Environment.NewLine,
                        matches.Item2.Select(x => string.Format("   {0}", x.Key.ToString())));

                    throw new IdResolutionException(
                        "Name '{0}' with {1} {2} ambiguously matches multiple contexts in {3}.{4}{5}",
                        name,
                        nameof(ScopeSet),
                        context.ToString(),
                        nameof(BindingStore),
                        System.Environment.NewLine,
                        formattedMatches
                        );
                }
                else
                {
                    return matches.Item2[0].Value;
                }
            }
            else
            {
                //throw new IdResolutionException(
                //    "Couldn't resolve name '{0}' in {2}.",
                //    name,
                //    nameof(BindingStore));
                return name;
            }
        }

    }
}
