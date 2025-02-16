using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Ops
{
    internal static class Conses
    {
        public static Term Cons(Term t1, Term t2) => Pair.Cons(t1, t2);
        public static Term Car(Pair p) => p.Car;
        public static Term Cdr(Pair p) => p.Cdr;

        public static Term SetCar(Pair p, Term t)
        {
            p.SetCar(t);
            return VoidTerm.Value;
        }

        public static Term SetCdr(Pair p, Term t)
        {
            p.SetCdr(t);
            return VoidTerm.Value;
        }
    }
}
