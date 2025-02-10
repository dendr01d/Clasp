using Clasp.Data.Metadata;
using Clasp.Data.Terms;
using Clasp.Data.Terms.Product;

namespace Clasp.Ops
{
    internal static class Pairs
    {
        public static Term Cons(MachineState mx, params Term[] terms)
        {
            return ConsList.Cons(terms[0], terms[1]);
        }

        public static Term Car(MachineState mx, params Term[] terms)
        {
            if (terms[0] is ConsList cl) return cl.Car;
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term Cdr(MachineState mx, params Term[] terms)
        {
            if (terms[0] is ConsList cl) return cl.Cdr;
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term SetCar(MachineState mx, params Term[] terms)
        {
            if (terms[0] is not ConsList cl) throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
            cl.SetCar(terms[1]);
            return VoidTerm.Value;
        }

        public static Term SetCdr(MachineState mx, params Term[] terms)
        {
            if (terms[0] is not ConsList cl) throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
            cl.SetCdr(terms[1]);
            return VoidTerm.Value;
        }

    }
}
