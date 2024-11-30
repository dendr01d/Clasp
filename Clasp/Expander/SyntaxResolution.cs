using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clasp.AST;

namespace Clasp.Expander
{
    internal static class SyntaxResolution
    {
        public static Fixed Resolve(Syntax stx, Dictionary<string, Dictionary<HashSet<string>, string>> bindingStore)
        {

        }

        private static Symbol ResolveIdentifier(SyntaxId id, Dictionary<string, Dictionary<HashSet<string>, string>> bindingStore)
        {
            if (bindingStore.TryGetValue(id.WrappedValue.Name, out Dictionary<HashSet<string>, string>? map))
            {
                HashSet<string> biggestSubsetScope = map
                    .MaxBy(x => x.Key.Intersect(id.Context).Count())
                    .Key;

                if (biggestSubsetScope.Count > 0)
                {
                    //return map[biggestSubsetScope];

                    //TODO this feels incorrect?
                    return Symbol.Intern(map[biggestSubsetScope]);
                }
            }

            return id.WrappedValue;
        }

    }
}
