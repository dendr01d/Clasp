using System;
using System.Collections.Generic;
using System.Linq;

using Clasp.Data.Terms.SyntaxValues;

namespace Clasp.Data.Terms.ProductValues
{
    internal sealed class Cons : Term
    {
        public Term Car { get; private set; }
        public Term Cdr { get; private set; }

        public static Cons Truct(Term car, Term cdr)
        {
            return new Cons(car, cdr);
        }

        private Cons(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public void SetCar(Term t) => Car = t;
        public void SetCdr(Term t) => Cdr = t;

        #region Aggregate Construction

        public static Term ProperList(params Term[] terms) => ProperList(terms.AsEnumerable());
        public static Term ProperList(IEnumerable<Term> terms)
        {
            Term output = Nil.Value;

            foreach (Term t in terms.Reverse())
            {
                output = Truct(t, output);
            }

            return output;
        }

        public static Term ImproperList(IEnumerable<Term> terms) => ImproperList(terms.ToArray());
        public static Term ImproperList(params Term[] terms)
        {
            Term output = terms[^1];

            for (int i = terms.Length - 2; i >= 0; --i)
            {
                output = Truct(terms[i], output);
            }

            return output;
        }

        #endregion

        public override string ToString() => string.Format("({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t)
        {
            return t switch
            {
                SyntaxPair stl => PrintAsTail(stl.Expose()),
                Cons cns => string.Format(" {0}{1}", cns.Car, PrintAsTail(cns.Cdr)),
                Nil => string.Empty,
                _ => string.Format(" . {0}", t)
            };
        }

        protected override string FormatType() => nameof(Cons);
    }
}
