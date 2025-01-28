using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Binding.Scopes
{
    internal enum ScopeType
    {
        TopLevel,
        Module,
        Body
    }


    /// <summary>
    /// Represents a specific lexical scope as represented by syntax, along with a map of renamed identifiers in that scope.
    /// </summary>
    /// <remarks>
    /// Not a 1:1 correspondence with scopes as they're used for differentiating syntactic bindings.
    /// </remarks>
    internal class BlockScope
    {
        private readonly uint _scopeToken;
        private readonly Dictionary<string, List<KeyValuePair<HashSet<uint>, ExpansionBinding>>> _renamings;

        private readonly BlockScope? _parent;
        private readonly Lazy<HashSet<uint>> _scopeSet;

        public uint Id => _scopeToken;
        public IEnumerable<uint> ScopeSet => _scopeSet.Value;
        public readonly ScopeType BlockType;

        private BlockScope(uint id, ScopeType type)
        {
            _scopeToken = id;
            _renamings = new();
            _parent = null;
            _scopeSet = new Lazy<HashSet<uint>>(AccumulateScopeSet);

            BlockType = type;
        }

        private BlockScope(uint id, ScopeType type, BlockScope parent) : this(id, type)
        {
            _parent = parent;
        }

        public static BlockScope MakeTopLevel(ScopeTokenGenerator gen)
        {
            return new BlockScope(gen.FreshToken(), ScopeType.TopLevel);
        }

        public static BlockScope MakeBody(ScopeTokenGenerator gen, BlockScope bScp)
        {
            return new BlockScope(gen.FreshToken(), ScopeType.Body, bScp);
        }

        public void AddBinding(string symName, IEnumerable<uint> scopeSet, ExpansionBinding binding)
        {
            if (!_renamings.ContainsKey(symName))
            {
                _renamings[symName] = new List<KeyValuePair<HashSet<uint>, ExpansionBinding>>();
            }

            _renamings[symName].Add(new KeyValuePair<HashSet<uint>, ExpansionBinding>(
                new HashSet<uint>(scopeSet),
                binding));
        }


        /// <summary>
        /// Return all bindings to which the <paramref name="symbolicName"/>
        /// may be bound in the context of the given <paramref name="scopeSet"/>.
        /// </summary>
        /// <remarks>
        /// May validly return zero, one, or more bindings.
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

        private static IEnumerable<BlockScope> EnumerateNestedScopes(BlockScope s)
        {
            BlockScope? current = s;
            while (current is BlockScope next)
            {
                yield return next;
                current = next._parent;
            }
            yield break;
        }

        private HashSet<uint> AccumulateScopeSet()
            => new HashSet<uint>(EnumerateNestedScopes(this).Select(x => x._scopeToken));

        private IEnumerable<KeyValuePair<HashSet<uint>, ExpansionBinding>> EnumerateRenamings(string symName)
        {
            foreach (BlockScope s in EnumerateNestedScopes(this))
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
