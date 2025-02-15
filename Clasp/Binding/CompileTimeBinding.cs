using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Binding
{
    internal enum BindingType
    {
        Variable,
        Transformer,
        Special,
        Primitive
    }

    internal class CompileTimeBinding
    {
        public Identifier Id { get; private set; }
        public BindingType BoundType { get; private set; }

        public Symbol BindingSymbol => Id.Expose();
        public string Name => Id.Name;

        public CompileTimeBinding(Identifier id, BindingType type)
        {
            Id = id;
            BoundType = type;
        }

        public override string ToString() => string.Format("{0}: {1}", BoundType, Name);
    }
}
