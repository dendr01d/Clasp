using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Binding.Environments;
using Clasp.Data;
using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Binding
{
    internal class BindingStore
    {
        private readonly Dictionary<string, List<KeyValuePair<HashSet<uint>, CompileTimeBinding>>> _renamings;

        public BindingStore(Environment env)
        {
            _renamings = new();

            foreach(var staticallyBound in env.TopLevel.StaticBindings)
            {
                if (staticallyBound.Value is Symbol)
                {
                    BindSpecialForm(staticallyBound.Key);
                }
                else if (staticallyBound.Value is PrimitiveProcedure)
                {
                    BindPrimitiveOperator(staticallyBound.Key);
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
        }

        public bool TryAddBinding(string symName, IEnumerable<uint> scopeSet, CompileTimeBinding binding)
        {
            if (!_renamings.ContainsKey(symName))
            {
                _renamings[symName] = new List<KeyValuePair<HashSet<uint>, CompileTimeBinding>>();
            }
            else if (_renamings[symName].Any(x => x.Key.SetEquals(scopeSet)))
            {
                return false;
            }

            _renamings[symName].Add(new KeyValuePair<HashSet<uint>, CompileTimeBinding>(
                new HashSet<uint>(scopeSet),
                binding));

            return true;
        }

        public bool TryAddBinding(Identifier id, int phase, CompileTimeBinding binding)
            => TryAddBinding(id.Name, id.GetScopeSet(phase), binding);

        private void BindSpecialForm(string name)
        {
            Identifier coreId = new Identifier(name, LexInfo.Innate);
            CompileTimeBinding binding = new CompileTimeBinding(coreId, BindingType.Special);
            if (!TryAddBinding(name, [], binding))
            {
                throw new ClaspGeneralException("Tried to rebind special form '{0}' at static level.", name);
            }
        }

        private void BindPrimitiveOperator(string name)
        {
            Identifier coreId = new Identifier(name, LexInfo.Innate);
            CompileTimeBinding binding = new CompileTimeBinding(coreId, BindingType.Primitive);
            if (!TryAddBinding(name, [], binding))
            {
                throw new ClaspGeneralException("Tried to rebind primitive operator '{0}' at static level.", name);
            }
        }

        /// <summary>
        /// Return all bindings to which the <paramref name="symbolicName"/>
        /// may be bound in the context of the given <paramref name="scopeSet"/>.
        /// </summary>
        /// <remarks>
        /// May validly return zero, one, or more bindings.
        /// </remarks>
        public IEnumerable<CompileTimeBinding> ResolveBindings(string symbolicName, uint[] scopeSet)
        {
            var results1 = _renamings.GetValueOrDefault(symbolicName); // Get all renames
            var results2 = results1?.Where(x => x.Key.Count() <= scopeSet.Length); // Limit to *subsets* of the given scope set
            var results3 = results2?.GroupBy(x => x.Key.Intersect(scopeSet).Count()); // Group them by subset size
            var results4 = results3?.MaxBy(x => x.Key); // Pick the group containing the largest subsets
            var results5 = results4?.Select(x => x.Value); // Collect the bindings
            return results5 ?? []; // Return none if there are no valid subsets
        }
    }
}
