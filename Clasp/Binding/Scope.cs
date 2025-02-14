
using System.Collections.Generic;

using Clasp.Data.Metadata;
using Clasp.Data.Terms.Syntax;

namespace Clasp.Binding
{
    internal class Scope : ISourceTraceable
    {
        private static uint _idCounter = 0;

        public readonly uint Id;

        public SourceCode Location { get; private set; }

        public readonly Dictionary<string, CompileTimeBinding> _bindingStore;

        public Scope(SourceCode loc)
        {
            Id = _idCounter++;
            Location = loc;
            _bindingStore = [];
        }

        public Scope(Syntax stx) : this(stx.Location) { }

        public void AddBinding(string symbolicName, CompileTimeBinding binding)
        {
            _bindingStore.Add(symbolicName, binding);
        }

        public bool TryResolve(string symbolicName, out CompileTimeBinding? bindingId)
        {
            return _bindingStore.TryGetValue(symbolicName, out bindingId);
        }

        public override string ToString()
        {
            return string.Format("Scope #{0} @ {1}", _id, Location.Snippet);
        }
    }
}
