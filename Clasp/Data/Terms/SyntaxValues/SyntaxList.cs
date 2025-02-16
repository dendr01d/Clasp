using System;

using Clasp.Binding;
using Clasp.Data.Metadata;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Data.Terms.SyntaxValues
{
    internal sealed class SyntaxList : Syntax
    {
        private readonly Cons<Syntax, Term> _list;
        public override Cons<Syntax, Term> Expose() => _list;

        public SyntaxList(Cons<Syntax, Term> list, LexInfo ctx) : base(ctx)
        {
            _list = list;
        }

        public SyntaxList(Syntax singleItem, LexInfo ctx) : base(ctx)
        {
            _list = Cons.Truct<Syntax, Term>(singleItem, Nil.Value);
        }

        public SyntaxList(Syntax car, Syntax dottedCdr, LexInfo ctx) : base(ctx)
        {
            _list = Cons.Truct<Syntax, Term>(car, dottedCdr);
        }

        public SyntaxList Push(Syntax newCar)
        {
            return new SyntaxList(Cons.Truct<Syntax, Term>(newCar, _list), LexContext);
        }

        public SyntaxList PopFront()
        {
            if (_list.Cdr is not Cons<Syntax, Term> remaining)
            {
                throw new ClaspGeneralException("Can't pop the front off of {0} -- then it wouldn't be a {0}!: {1}", nameof(SyntaxList), _list);
            }

            return new SyntaxList(remaining, LexContext);
        }

        public static SyntaxList ProperList(LexInfo info, Syntax first, params Syntax[] rest)
        {
            return new SyntaxList(SpinProperList(first, rest), info);
        }
        private static Cons<Syntax, Term> SpinProperList(Syntax first, params Syntax[] elements)
        {
            if (elements.Length == 0)
            {
                return new Cons<Syntax, Term>(first, Nil.Value);
            }
            else
            {
                Cons<Syntax, Term> cdr = SpinProperList(elements[0], elements[1..]);
                return new Cons<Syntax, Term>(first, cdr);
            }
        }

        public static SyntaxList ImproperList(LexInfo info, Syntax first, Syntax second, params Syntax[] rest)
        {
            return new SyntaxList(SpinImproperList(first, second, rest), info);
        }
        private static Cons<Syntax, Term> SpinImproperList(Syntax first, Syntax second, params Syntax[] more)
        {
            if (more.Length == 0)
            {
                return new Cons<Syntax, Term>(first, second);
            }
            else
            {
                Cons<Syntax, Term> cdr = SpinImproperList(second, more[0], more[1..]);
                return new Cons<Syntax, Term>(first, cdr);
            }
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

        private static void EagerlyRecur(Cons<Syntax, Term> stp, Action<Syntax> fun)
        {
            Term current = stp;

            while (current is Cons<Syntax, Term> next)
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
