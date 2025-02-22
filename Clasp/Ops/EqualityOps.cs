﻿using System.Collections;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.ProductValues;

namespace Clasp.Ops
{
    internal static class EqualityOps
    {
        public static Term Eq(Term t1, Term t2) => t1 == t2;

        public static Term Eqv(Term t1, Term t2)
        {
            return (t1, t2) switch
            {
                (Number n1, Number n2) => MathOps.Equivalent(n1, n2),
                (Character c1, Character c2) => c1.Value == c2.Value,
                (Boolean b1, Boolean b2) => b1.Value == b2.Value,
                _ => Eq(t1, t2)
            };
        }

        public static Term Equal(Term t1, Term t2)
        {
            return (t1, t2) switch
            {
                (CharString cs1, CharString cs2) => string.Equals(cs1.Value, cs2.Value),
                (Vector v1, Vector v2) => v1.Values.Zip(v2.Values, Equal).All(x => x),
                (Cons p1, Cons p2) => Equal(p1.Car, p2.Car) && Equal(p1.Cdr, p2.Cdr),
                _ => Eqv(t1, t2)
            };
        }
    }
}
