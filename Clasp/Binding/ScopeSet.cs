using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.Immutable;

namespace Clasp.Binding
{
    /// <summary>
    /// Represents a logical scoping of uints
    /// </summary>
    internal class ScopeSet
    {
        private readonly HashSet<uint> _scopes;

        public int ScopeSize => _scopes.Count;

        /// <summary>
        /// Construct a fresh context that's nested inside zero uints.
        /// </summary>
        public ScopeSet()
        {
            _scopes = new HashSet<uint>();
        }

        /// <summary>
        /// Construct a fresh context with identical lexical scoping to the provided one.
        /// </summary>
        /// <param name="existing"></param>
        public ScopeSet(ScopeSet existing)
        {
            _scopes = new HashSet<uint>(existing._scopes);
        }

        /// <summary>
        /// Create a fresh context with identical lexical scoping to this one, with the addition of the provided tokens
        /// </summary>
        public ScopeSet Extend(params uint[] tokens)
        {
            ScopeSet output = new ScopeSet(this);
            output.Add(tokens);
            return output;
        }

        /// <summary>
        /// Expand the scope set by adding the given token to the set.
        /// </summary>
        public void Add(params uint[] tokens)
        {
            _scopes.UnionWith(tokens);
        }

        public void Add(ScopeSet other)
        {
            _scopes.UnionWith(other._scopes);
        }

        /// <summary>
        /// For each provided token, add it to the scope set if the set doesn't already contain it. Else remove it.
        /// </summary>
        public void Flip(params uint[] tokens)
        {
            _scopes.SymmetricExceptWith(tokens);
        }

        /// <summary>
        /// Returns how many elements of <paramref name="superSet"/> are also contained in this set.
        /// </summary>
        public int SubsetSize(ScopeSet superSet)
        {
            return _scopes.Intersect(superSet._scopes).Count();
        }

        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(", ", _scopes));
        }
    }
}
