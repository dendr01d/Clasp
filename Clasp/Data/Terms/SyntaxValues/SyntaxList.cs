using System;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal sealed class SyntaxList : Syntax
    {
        private readonly StxPair _list;
        public override StxPair Expose() => _list;

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

        public SyntaxList Push(Syntax newCar)
        {
            return new SyntaxList(StxPair.Cons(newCar, _list), LexContext);
        }

        public SyntaxList PopFront()
        {
            if (_list.Cdr is not StxPair remaining)
            {
                throw new ClaspGeneralException("Can't pop the front off of {0} -- then it wouldn't be a {0}!: {1}", nameof(SyntaxList), _list);
            }

            return new SyntaxList(remaining, LexContext);
        }

        public override void AddScope(int phase, params Scope[] scopes)
        {
            LexContext.AddScope(phase, scopes);
            EagerlyRecur(_list, x => x.AddScope(phase, scopes));
        }

        public override void FlipScope(int phase, params Scope[] scopes)
        {
            LexContext.FlipScope(phase, scopes);
            EagerlyRecur(_list, x => x.FlipScope(phase, scopes));
        }

        public override void RemoveScope(int phase, params Scope[] scopes)
        {
            LexContext.RemoveScope(phase, scopes);
            EagerlyRecur(_list, x => x.RemoveScope(phase, scopes));
        }

        private static void EagerlyRecur(StxPair stp, Action<Syntax> fun)
        {
            Term current = stp;

            while (current is StxPair next)
            {
                fun(next.Car);
                current = next.Cdr;
            }

            if (current is Syntax stx)
            {
                fun(stx);
            }
        }

        //public override string ToString() => string.Format("#'{0}", _list.ToString());
        protected override string FormatType() => string.Format("StxPair<{0}, {1}>", _list.Car.TypeName, _list.Cdr.TypeName);
        internal override string DisplayDebug() => string.Format("{0} ({1}): {2}", nameof(SyntaxList), nameof(Syntax), _list.ToString());

    }
}
