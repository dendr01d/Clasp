using System.Collections;
using System.Linq;

using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;

namespace Clasp.Ops
{
    internal static class Predicates
    {
        public static Term IsType<T>(MachineState mx, params Term[] args)
            where T : Term
        {
            return args.Length switch
            {
                1 => Helpers.AsTerm(args[0] is T),
                _ => throw new ProcessingException.InvalidPrimitiveArgumentsException(args)
            };
        }

        public static Term Eq(MachineState mx, params Term[] args)
        {
            return args.Length switch
            {
                2 => Helpers.AsTerm(args[0] == args[1]),
                _ => throw new ProcessingException.InvalidPrimitiveArgumentsException(args)
            };
        }

        public static Term Eqv(MachineState mx, params Term[] args)
        {
            if (args.Length != 2)
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
            }
            else
            {
                return (args[0], args[1]) switch
                {
                    (Integer i1, Integer i2) => Helpers.AsTerm(i1.Value == i2.Value),
                    (Real r1, Real r2) => Helpers.AsTerm(r1.Value == r2.Value),
                    (Character c1, Character c2) => Helpers.AsTerm(c1.Value == c2.Value),
                    (Boolean b1, Boolean b2) => Helpers.AsTerm(b1.Value == b2.Value),
                    (_, _) => Eq(mx, args)
                };
            }
        }

        public static Term Equal(MachineState mx, params Term[] args)
        {
            if (args.Length != 2)
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException(args);
            }
            else
            {
                return (args[0], args[1]) switch
                {
                    (CharString s1, CharString s2) => Helpers.AsTerm(string.Equals(s1.Value, s2.Value)),
                    (Vector v1, Vector v2) => Helpers.AsTerm(VecEqual(mx, v1, v2)),
                    (ConsList cl1, ConsList cl2) => Helpers.AsTerm(
                        (Equal(mx, cl1.Car, cl2.Car) == Boolean.True) && (Equal(mx, cl1.Cdr, cl2.Cdr) == Boolean.True)),
                    (_, _) => Eqv(mx, args)
                };
            }
        }

        private static bool VecEqual(MachineState mx, Vector v1, Vector v2)
        {
            if (v1.Values.Length != v2.Values.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < v1.Values.Length; ++i)
                {
                    if (Boolean.False == Equal(mx, v1.Values[i], v2.Values[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
