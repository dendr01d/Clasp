using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    /// <summary>
    /// Represents a logical scoping of environments
    /// </summary>
    internal class ScopeSet
    {
        private readonly HashSet<Environment> _scope;

        public int ScopeSize => _scope.Count;

        /// <summary>
        /// Construct a fresh context that's nested inside zero environments.
        /// </summary>
        public ScopeSet()
        {
            _scope = new HashSet<Environment>();
        }

        /// <summary>
        /// Constructs a context nexted inside the same environments as the provided existing context.
        /// </summary>
        /// <param name="existing"></param>
        public ScopeSet(ScopeSet existing)
        {
            _scope = new HashSet<Environment>(existing._scope);
        }

        /// <summary>
        /// Creates a new Context extended to be nested within the scope of the provided environment.
        /// </summary>
        public ScopeSet Extend(Environment newScope)
        {
            ScopeSet output = new ScopeSet(this);
            output._scope.Add(newScope);
            return output;
        }

        /// <summary>
        /// Returns how many elements of <paramref name="superSet"/> are also contained in this set.
        /// </summary>
        public int SubsetSize(ScopeSet superSet)
        {
            return _scope.Intersect(superSet._scope).Count();
        }

        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(", ", _scope.Select(x => string.Format("({0})", x.Count()))));
        }
    }
}
