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
        TopLevel
    }

    internal class ExpansionBinding
    {
        public readonly Identifier BindingIdentifier;
        public readonly BindingType BoundType;

        public ExpansionBinding(Identifier id, BindingType type)
        {
            BindingIdentifier = id;
            BoundType = type;
        }
    }
}
