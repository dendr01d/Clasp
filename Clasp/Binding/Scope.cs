using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    using ScopeSet = HashSet<uint>;
    using BoundName = KeyValuePair<HashSet<uint>, string>;
    using BindingMap = List<KeyValuePair<HashSet<uint>, string>>;
    using SymbolicMap = Dictionary<string, List<KeyValuePair<HashSet<uint>, string>>>;

    /// <summary>
    /// Essentially just an extremely convoluted way of mapping names to other names, depending
    /// on which nested syntactic structure they live in.
    /// </summary>
    internal class Scope : IComparable<Scope>
    {
        private static uint _globalCounter = 0;

        public readonly uint Id;
        private readonly SymbolicMap _bindings;

        public Scope()
        {
            Id = _globalCounter++;
            _bindings = new SymbolicMap();
        }

        public int CompareTo(Scope? other) => Id.CompareTo(other?.Id);
    }

    internal class RepresentativeScope : Scope
    {
        public readonly MultiScope Owner;
        public readonly uint Phase;

        public RepresentativeScope(MultiScope owner, uint phase)
        {
            Owner = owner;
            Phase = phase;
        }
    }
}
