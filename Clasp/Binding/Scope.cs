using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;

namespace Clasp.Binding
{
    /// <summary>
    /// Represents a lexical scope as represented by syntax,
    /// along with a map of renamed identifiers
    /// </summary>
    /// <remarks>
    /// Isn't this just a subtly different environment structure???
    /// Is that... okay??
    /// </remarks>
    internal class Scope
    {
        private readonly ScopeToken _id;
        private Dictionary<string, List<ScopedBindingName>> _renamings;

        private Scope? _parent;
        private Lazy<ScopeTokenSet> _scopeSet;

        public IEnumerable<ScopeToken> ScopeSet => _scopeSet.Value;

        public Scope(ScopeToken id)
        {
            _id = id;
            _renamings = new Dictionary<string, List<ScopedBindingName>>();
            _parent = null;
            _scopeSet = new Lazy<ScopeTokenSet>(AccumulateScopeSet);
        }

        public Scope(ScopeToken id, Scope parent) : this(id)
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
        public IEnumerable<string> ResolveBindingNames(string symbolicName, HashSet<uint> scopeSet)
        {
            return EnumerateRenamings(symbolicName) // Get all renames
                .Where(x => x.Key.Count <= scopeSet.Count) // Limit to subsets of the given scope set
                .GroupBy(x => x.Key.Intersect(scopeSet).Count()) // Group them by subset size
                .MaxBy(x => x.Key) // Pick the group containing the largest subsets
                ?.Select(x => x.Value) // Collect the binding names
                ?? []; // Return none if there are no valid subsets
        }

        #region Inter-Scope Operations

        private static IEnumerable<Scope> EnumerateNestedScopes(Scope s)
        {
            Scope? current = s;
            while (current is Scope next)
            {
                yield return next;
                current = next;
            }
            yield break;
        }

        private ScopeTokenSet AccumulateScopeSet()
            => new ScopeTokenSet(EnumerateNestedScopes(this).Select(x => x._id));

        private IEnumerable<ScopedBindingName> EnumerateRenamings(string symName)
        {
            foreach(Scope s in EnumerateNestedScopes(this))
            {
                if (s._renamings.TryGetValue(symName, out List<ScopedBindingName>? sbns))
                {
                    foreach(var scopedName in sbns)
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
