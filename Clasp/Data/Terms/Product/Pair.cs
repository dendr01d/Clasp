using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.ExtensionMethods;
using Clasp.Interfaces;

namespace Clasp.Data.Terms.Product
{
    internal class Pair : Product, ICons<Term, Term>
    {
        public virtual Term Car { get; private set; }
        public Term Cdr { get; private set; }

        public bool IsDotted => Cdr is not Pair or Nil;

        public void SetCar(Term newCar) => Car = newCar;
        public void SetCdr(Term newCdr) => Cdr = newCdr;

        private Pair(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }
        public IEnumerator<Term?> GetEnumerator() => this.EnumerateElements().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.EnumerateElements().GetEnumerator();

        #region Static Construction

        public static Pair Cons(Term car, Term cdr) => new Pair(car, cdr);

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

        #endregion

        public override string ToString() => string.Format("({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t)
        {
            return t switch
            {
                Pair cl => string.Format(" {0}{1}", cl.Car, PrintAsTail(cl.Cdr)),
                Nil => string.Empty,
                _ => string.Format(" . {0}", t)
            };
        }

        protected override string FormatType() => string.Format("Cons<{0}, {1}>", Car.TypeName, Cdr.TypeName);

    }
}
