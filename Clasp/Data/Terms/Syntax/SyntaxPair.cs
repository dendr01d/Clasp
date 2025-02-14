using System.Collections;
using System.Collections.Generic;

using Clasp.Data.Metadata;
using Clasp.Data.Terms.Product;
using Clasp.ExtensionMethods;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.Syntax
{
    internal sealed class SyntaxPair : Syntax
    {
        private Pair _list;
        public override Pair Expose() => _list;

        public SyntaxPair(Pair list, LexInfo ctx) : base(ctx)
        {
            _list = list;
        }

        public override string ToString() => _list.ToString();
        protected override string FormatType() => string.Format("StxPair<{0}, {1}>", _list.Car.TypeName, _list.Cdr.TypeName);
    }
}
