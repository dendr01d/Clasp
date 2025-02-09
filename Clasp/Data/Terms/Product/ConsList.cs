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
    internal class ConsList : Product, ICons<Term, Term>
    {
        public virtual Term Car { get; private set; }
        public Term Cdr { get; private set; }

        public bool IsDotted => Cdr is not ConsList or Nil;

        public void SetCar(Term newCar) => Car = newCar;
        public void SetCdr(Term newCdr) => Cdr = newCdr;

        private ConsList(Term car, Term cdr)
        {
            Car = car;
            Cdr = cdr;
        }
        public IEnumerator<Term?> GetEnumerator() => this.EnumerateElements().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.EnumerateElements().GetEnumerator();

        #region Static Construction

        public static ConsList Cons(Term car, Term cdr) => new ConsList(car, cdr);

        public static Term ProperList(params Term[] terms) => ProperList(terms.AsEnumerable());
        public static Term ProperList(IEnumerable<Term> terms)
        {
            Term output = Nil.Value;

            foreach (Term t in terms.Reverse())
            {
                output = new ConsList(t, output);
            }

            return output;
        }

        public static ConsList ImproperList(Term first, Term second, params Term[] rest)
        {
            if (rest.Length == 0)
            {
                return new ConsList(first, second);
            }
            else
            {
                ConsList cdr = ImproperList(second, rest[0], rest[1..]);
                return new ConsList(first, cdr);
            }
        }

        public static ConsList ConstructDirect(IEnumerable<Term> terms)
        {
            Term[] realized = terms.ToArray();

            if (realized.Length < 2)
            {
                throw new ClaspGeneralException("At least two terms needed to construct a cons cell.");
            }
            else
            {
                return ImproperList(realized[0], realized[1], realized[2..]);
            }
        }

        #endregion

        public override string ToString() => string.Format("({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t)
        {
            return t switch
            {
                ConsList cl => string.Format(" {0}{1}", cl.Car, PrintAsTail(cl.Cdr)),
                Nil => string.Empty,
                _ => string.Format(" . {0}", t)
            };
        }

        protected override string FormatType() => string.Format("Cons<{0}, {1}>", Car.TypeName, Cdr.TypeName);

    }
}
