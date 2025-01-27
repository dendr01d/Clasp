using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding.Scopes
{
    /// <summary>
    /// Represents a specific lexical scope as represented by syntax, along with a map of renamed identifiers in that scope.
    /// </summary>
    internal class Scope
    {
        private readonly uint _id;
        private readonly Dictionary<string, List<KeyValuePair<HashSet<uint>, ExpansionBinding>>> _renamings;

        private readonly Scope? _parent;
        private readonly Lazy<HashSet<uint>> _scopeSet;

        public uint Id => _id;
        public IEnumerable<uint> ScopeSet => _scopeSet.Value;

        public Scope(uint id)
        {
            _id = id;
            _renamings = new();
            _parent = null;
            _scopeSet = new Lazy<HashSet<uint>>(AccumulateScopeSet);
        }

        public Scope(uint id, Scope parent) : this(id)
        {
            _parent = parent;
        }

        /// <summary>
        /// Return all binding names to which the <paramref name="symbolicName"/>
        /// could be bound to in the context of the given <paramref name="scopeSet"/>.
        /// </summary>
        /// <remarks>
        /// May validly return zero, one, or more names.
        /// </remarks>
        public IEnumerable<ExpansionBinding> ResolveBindings(string symbolicName, HashSet<uint> scopeSet)
        {
            return EnumerateRenamings(symbolicName) // Get all renames
                .Where(x => x.Key.Count() <= scopeSet.Count) // Limit to *subsets* of the given scope set
                .GroupBy(x => x.Key.Intersect(scopeSet).Count()) // Group them by subset size
                .MaxBy(x => x.Key) // Pick the group containing the largest subsets
                ?.Select(x => x.Value) // Collect the bindings
                ?? []; // Return none if there are no valid subsets
        }

        #region Inter-Scope Operations

        private static IEnumerable<Scope> EnumerateNestedScopes(Scope s)
        {
            Scope? current = s;
            while (current is Scope next)
            {
                yield return next;
                current = next._parent;
            }
            yield break;
        }

        private HashSet<uint> AccumulateScopeSet()
            => new HashSet<uint>(EnumerateNestedScopes(this).Select(x => x._id));

        private IEnumerable<KeyValuePair<HashSet<uint>, ExpansionBinding>> EnumerateRenamings(string symName)
        {
            foreach (Scope s in EnumerateNestedScopes(this))
            {
                if (s._renamings.TryGetValue(symName, out List<KeyValuePair<HashSet<uint>, ExpansionBinding>>? scopedBindings))
                {
                    foreach (var scopedName in scopedBindings)
                    {
                        yield return scopedName;
                    }
                }
            }
            yield break;
        }

        #endregion
    }
}
