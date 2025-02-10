using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Helpers
    {
        public static Term FoldLeft(MachineState mx, System.Func<MachineState, Term, Term, Term> op, params Term[] terms)
        {
            if (terms.Length == 0)
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException();
            }

            Term result = terms[0];

            for (int i = 1; i < terms.Length; ++i)
            {
                result = op(mx, result, terms[i]);
            }

            return result;
        }

        public static Term AsTerm(bool b) => b ? Boolean.True : Boolean.False;

    }
}
