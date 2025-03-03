
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Clasp.Data.Metadata;
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

        public readonly Dictionary<string, ExpansionVarNameBinding> _bindingStore;

        public Scope(SourceCode loc)
        {
            Id = _idCounter++;
            Location = loc;
            _bindingStore = [];
        }

        public Scope(Syntax stx) : this(stx.Location) { }

        public bool Binds(string symbolicName) => _bindingStore.ContainsKey(symbolicName);

        /// <summary>
        /// Create a <paramref name="type"/> <see cref="ExpansionVarNameBinding"/> on
        /// <paramref name="symbolicName"/> that binds to itself as an <see cref="Identifier"/>.
        /// </summary>
        public void AddStaticBinding(string symbolicName, BindingType type)
        {
            Identifier newId = new Identifier(symbolicName, ScopeSet.StaticInfo);
            ExpansionVarNameBinding newBinding = new ExpansionVarNameBinding(newId, type);
            AddBinding(symbolicName, newBinding);
        }

        public void AddBinding(string symbolicName, ExpansionVarNameBinding binding)
        {
            _bindingStore.Add(symbolicName, binding);
        }

        public bool TryResolve(string symbolicName,
            [NotNullWhen(true)] out ExpansionVarNameBinding? bindingId)
        {
            return _bindingStore.TryGetValue(symbolicName, out bindingId);
        }

        public override string ToString()
        {
            return string.Format("Scope #{0} @ {1}", Id, Location);
        }
    }
}
