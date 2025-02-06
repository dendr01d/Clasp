using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Binding
{
    internal class BindingStore
    {
        private readonly Dictionary<string, List<KeyValuePair<HashSet<uint>, CompileTimeBinding>>> _renamings;

        public BindingStore()
        {
            _renamings = new();

            BindCoreForm(Keyword.DEFINE);
            BindCoreForm(Keyword.DEFINE_SYNTAX);
            BindCoreForm(Keyword.LET_SYNTAX);
            BindCoreForm(Keyword.LAMBDA);
        }

        public void AddBinding(string symName, IEnumerable<uint> scopeSet, CompileTimeBinding binding)
        {
            if (!_renamings.ContainsKey(symName))
            {
                _renamings[symName] = new List<KeyValuePair<HashSet<uint>, CompileTimeBinding>>();
            }

            _renamings[symName].Add(new KeyValuePair<HashSet<uint>, CompileTimeBinding>(
                new HashSet<uint>(scopeSet),
                binding));
        }

        public void AddBinding(Identifier id, int phase, CompileTimeBinding binding)
            => AddBinding(id.Name, id.GetScopeSet(phase), binding);

        private void BindCoreForm(string name)
        {
            Identifier coreId = new Identifier(name, SourceLocation.InherentSource);
            CompileTimeBinding binding = new CompileTimeBinding(coreId, BindingType.Special);
            AddBinding(name, [], binding);
        }


        /// <summary>
        /// Return all bindings to which the <paramref name="symbolicName"/>
        /// may be bound in the context of the given <paramref name="scopeSet"/>.
        /// </summary>
        /// <remarks>
        /// May validly return zero, one, or more bindings.
        /// </remarks>
        public IEnumerable<CompileTimeBinding> ResolveBindings(string symbolicName, HashSet<uint> scopeSet)
        {
            return _renamings
                .GetValueOrDefault(symbolicName)// Get all renames
                ?.Where(x => x.Key.Count() <= scopeSet.Count) // Limit to *subsets* of the given scope set
                ?.GroupBy(x => x.Key.Intersect(scopeSet).Count()) // Group them by subset size
                ?.MaxBy(x => x.Key) // Pick the group containing the largest subsets
                ?.Select(x => x.Value) // Collect the bindings
                ?? []; // Return none if there are no valid subsets
        }
    }
}
