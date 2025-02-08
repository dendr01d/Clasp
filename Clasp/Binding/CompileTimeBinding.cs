using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Terms;
using Clasp.Data.Terms.Syntax;

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
        public Identifier BoundId { get; private set; }
        public BindingType BoundType { get; private set; }

        public Symbol BindingSymbol => BoundId.Expose();
        public string BindingName => BoundId.Name;

        public CompileTimeBinding(Identifier id, BindingType type)
        {
            BoundId = id;
            BoundType = type;
        }
    }
}
