using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal sealed class SyntaxList : Syntax
    {
        private readonly StxPair _list;
        public override StxPair Expose() => _list;

        public Term Car => _list.Car;

        public SyntaxList(StxPair list, LexInfo ctx) : base(ctx)
        {
            _list = list;
        }

        public SyntaxList(Syntax singleItem, LexInfo ctx) : base(ctx)
        {
            _list = StxPair.Cons(singleItem, Nil.Value);
        }

        public SyntaxList(Syntax car, Syntax dottedCdr, LexInfo ctx) : base(ctx)
        {
            _list = StxPair.Cons(car, dottedCdr);
        }

        public SyntaxList Prepend(Syntax car)
        {
            return new SyntaxList(StxPair.Cons(car, _list), LexContext);
        }

        public override string ToString() => _list.ToString();
        protected override string FormatType() => string.Format("StxPair<{0}, {1}>", _list.Car.TypeName, _list.Cdr.TypeName);

    }
}
