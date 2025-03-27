using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class EqualityOps
    {
        public static ITerm Eq(ITerm t1, ITerm t2) => new Boole(ReferenceEquals(t1, t2));

        public static ITerm Eqv(ITerm t1, ITerm t2)
        {
            return (t1, t2) switch
            {
                (FixNum fix1, FixNum fix2) => new Boole(fix1.Equals(fix2)),
                (FloNum flo1, FloNum flo2) => new Boole(flo1.Equals(flo2)),
                (Character c1, Character c2) => new Boole(c1.Equals(c2)),
                (Boole b1, Boole b2) => new Boole(b1.Equals(b2)),
                _ => Eq(t1, t2)
            };
        }

        public static ITerm Equal(ITerm t1, ITerm t2)
        {
            return (t1, t2) switch
            {
                (RefString cs1, RefString cs2) => new Boole(cs1.Equals(cs2)),
                (Vector v1, Vector v2) => new Boole(v1.Equals(v2)),
                (Cons p1, Cons p2) => new Boole(p1.Equals(p2)),
                _ => Eqv(t1, t2)
            };
        }
    }
}
