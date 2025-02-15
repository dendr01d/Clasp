
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Terms.SyntaxValues;
using Clasp.Data.Text;
using Clasp.Interfaces;

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

        public bool Binds(string symbolicName) => _bindingStore.ContainsKey(symbolicName);

        public void AddBinding(string symbolicName, CompileTimeBinding binding)
        {
            _bindingStore.Add(symbolicName, binding);
        }

        public bool TryResolve(string symbolicName,
            [NotNullWhen(true)] out CompileTimeBinding? bindingId)
        {
            return _bindingStore.TryGetValue(symbolicName, out bindingId);
        }

        public override string ToString()
        {
            return string.Format("Scope #{0} @ {1}", Id, Location.Snippet);
        }
    }
}
