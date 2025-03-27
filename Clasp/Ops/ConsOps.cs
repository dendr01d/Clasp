using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class ConsOps
    {
        public static ITerm Cons(ITerm t1, ITerm t2) => new Cons(t1, t2);
        public static ITerm Car(Cons p) => p.Car;
        public static ITerm Cdr(Cons p) => p.Cdr;

        public static ITerm SetCar(Cons p, ITerm t)
        {
            p.SetCar(t);
            return new VoidResult();
        }

        public static ITerm SetCdr(Cons p, ITerm t)
        {
            p.SetCdr(t);
            return new VoidResult();
        }
    }
}
