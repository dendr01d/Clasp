using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Data.Terms.ProductValues
{
    internal abstract class Cons : Term
    {
        public abstract Term Car { get; }
        public abstract Term Cdr { get; }
    }

    internal abstract class Cons<T1, T2> : Cons
        where T1 : Term
        where T2 : Term
    {
        private T1 _car;
        private T2 _cdr;

        public override T1 Car => _car;
        public override T2 Cdr => _cdr;

        protected Cons(T1 car, T2 cdr)
        {
            _car = car;
            _cdr = cdr;
        }

        public void SetCar(T1 newCar) => _car = newCar;
        public void SetCdr(T2 newCdr) => _cdr = newCdr;

        public override string ToString() => string.Format("({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t)
        {
            return t switch
            {
                Cons cns => string.Format(" {0}{1}", cns.Car, PrintAsTail(cns.Cdr)),
                Nil => string.Empty,
                _ => string.Format(" . {0}", t)
            };
        }

        protected override string FormatType() => string.Format("Cons<{0}, {1}>", Car.TypeName, Cdr.TypeName);
    }

    internal sealed class Pair : Cons<Term, Term>
    {
        private Pair(Term car, Term cdr) : base(car, cdr) { }

        public static Pair Cons(Term t1, Term t2) => new Pair(t1, t2);

        public static Term ProperList(params Term[] terms) => ProperList(terms.AsEnumerable());
        public static Term ProperList(IEnumerable<Term> terms)
        {
            Term output = Nil.Value;

            foreach (Term t in terms.Reverse())
            {
                output = new Pair(t, output);
            }

            return output;
        }

        public static Term ImproperList(params Term[] terms)
        {
            Term output = terms[^1];

            for (int i = terms.Length - 2; i >= 0; --i)
            {
                output = Cons(terms[i], output);
            }

            return output;
        }
    }

    internal sealed class StxPair : Cons<Syntax, Term>
    {
        private StxPair(Syntax car, Term cdr) : base(car, cdr) { }

        public static StxPair Cons(Syntax car, Nil cdr) => new StxPair(car, cdr);
        public static StxPair Cons(Syntax car, Syntax cdr) => new StxPair(car, cdr);
        public static StxPair Cons(Syntax car, StxPair cdr) => new StxPair(car, cdr);

        public static StxPair ProperList(Syntax first, params Syntax[] rest)
        {
            if (rest.Length == 0)
            {
                return Cons(first, Nil.Value);
            }
            else
            {
                return Cons(first, ProperList(rest[0], rest[1..]));
            }
        }

        public static StxPair ImproperList(Syntax first, params Syntax[] rest)
        {
            if (rest.Length < 1)
            {
                throw new ClaspGeneralException("Can't create improper {0} without at least 2 arguments.", nameof(StxPair));
            }
            else if (rest.Length == 1)
            {
                return StxPair.Cons(first, rest[0]);
            }
            else
            {
                StxPair cdr = ImproperList(rest[0], rest[1..]);
                return StxPair.Cons(first, cdr);
            }
        }
    }
}
