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

        public static Cons<T1, T2> Truct<T1, T2>(T1 car, T2 cdr)
            where T1 : Term
            where T2 : Term
        {
            return new Cons<T1, T2>(car, cdr);
        }

        public static Term ProperList<T>(params T[] terms)
            where T : Term
            => ProperList(terms.AsEnumerable());
        public static Term ProperList<T>(IEnumerable<T> terms)
            where T : Term
        {
            Term output = Nil.Value;

            foreach (T t in terms.Reverse())
            {
                output = Truct(t, output);
            }

            return output;
        }

        public static Term ImproperList<T>(IEnumerable<T> terms)
            where T : Term
            => ImproperList(terms.ToArray());
        public static Term ImproperList<T>(params T[] terms)
            where T : Term
        {
            Term output = terms[^1];

            for (int i = terms.Length - 2; i >= 0; --i)
            {
                output = Truct(terms[i], output);
            }

            return output;
        }

        public override string ToString() => string.Format("({0}{1})", Car, PrintAsTail(Cdr));

        private static string PrintAsTail(Term t)
        {
            return t switch
            {
                SyntaxList stl => PrintAsTail(stl.Expose()),
                Cons cns => string.Format(" {0}{1}", cns.Car, PrintAsTail(cns.Cdr)),
                Nil => string.Empty,
                _ => string.Format(" . {0}", t)
            };
        }
    }

    internal sealed class Cons<T1, T2> : Cons
        where T1 : Term
        where T2 : Term
    {
        private T1 _car;
        private T2 _cdr;

        public override T1 Car => _car;
        public override T2 Cdr => _cdr;

        public Cons(T1 car, T2 cdr)
        {
            _car = car;
            _cdr = cdr;
        }

        public void SetCar(T1 newCar) => _car = newCar;
        public void SetCdr(T2 newCdr) => _cdr = newCdr;

        protected override string FormatType() => string.Format("Cons<{0}, {1}>", Car.TypeName, Cdr.TypeName);

        internal override string DisplayDebug()
        {
            return string.Format("{0}<{1}, {2}>: {3}",
                nameof(Cons),
                typeof(T1).Name,
                typeof(T2).Name,
                ToString());
        }
    }
}
