using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Ops
{
    internal static class Conses
    {
        public static Term Cons(Term t1, Term t2) => Clasp.Data.Terms.ProductValues.Cons.Truct(t1, t2);
        public static Term Car(Cons p) => p.Car;
        public static Term Cdr(Cons p) => p.Cdr;

        public static Term SetCar(Cons<Term, Term> p, Term t)
        {
            p.SetCar(t);
            return VoidTerm.Value;
        }

        public static Term SetCdr(Cons<Term, Term> p, Term t)
        {
            p.SetCdr(t);
            return VoidTerm.Value;
        }
    }
}
