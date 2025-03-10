
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
        public readonly string Name;

        public SourceCode Location { get; private set; }

        public readonly Dictionary<string, RenameBinding> _bindingStore;

        public Scope(string name, SourceCode loc)
        {
            Id = _idCounter++;
            Name = name;

            Location = loc;
            _bindingStore = [];
        }

        //public Scope(Syntax stx) : this(stx.Location) { }

        public bool Binds(string symbolicName) => _bindingStore.ContainsKey(symbolicName);

        /// <summary>
        /// Create a <paramref name="type"/> <see cref="RenameBinding"/> on
        /// <paramref name="symbolicName"/> that binds to itself as an <see cref="Identifier"/>.
        /// </summary>
        public void AddStaticBinding(string symbolicName, BindingType type)
        {
            Identifier newId = new Identifier(symbolicName, SourceCode.StaticSource);
            RenameBinding newBinding = new RenameBinding(newId, type);
            AddBinding(symbolicName, newBinding);
        }

        public void AddBinding(string symbolicName, RenameBinding binding)
        {
            _bindingStore.Add(symbolicName, binding);
        }

        public bool TryResolve(string symbolicName,
            [NotNullWhen(true)] out RenameBinding? bindingId)
        {
            return _bindingStore.TryGetValue(symbolicName, out bindingId);
        }

        public override string ToString()
        {
            return string.Format("Scope #{0} \"{1}\" @ {2}", Id, Name, Location);
        }
    }
}
