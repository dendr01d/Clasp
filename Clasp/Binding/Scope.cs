using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Binding
{
    using ScopeMap = List<KeyValuePair<HashSet<uint>, string>>;

    internal class Scope
    {
        private static uint _globalCounter = 0;

        public readonly uint Id;
        private readonly ScopeMap _bindings;

        public Scope()
        {
            Id = _globalCounter++;
            _bindings = new ScopeMap();
        }

        public Scope(IEnumerable<KeyValuePair<HashSet<uint>, string>> bindings) : this()
        {
            _bindings = bindings.ToList();
        }

    }
}
