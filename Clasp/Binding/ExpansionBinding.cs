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
        Special
    }

    internal class ExpansionBinding
    {
        public readonly Identifier BoundId;
        public readonly BindingType BoundType;

        public Symbol BindingSymbol => BoundId.Expose();
        public string BindingName => BoundId.SymbolicName;

        public ExpansionBinding(Identifier id, BindingType type)
        {
            BoundId = id;
            BoundType = type;
        }
    }
}
