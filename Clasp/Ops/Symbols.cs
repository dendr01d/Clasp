using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Symbols
    {
        public static Term SymbolToString(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Symbol sym)
            {
                return new CharString(sym.Name);
            }
            else
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
            }
        }

        public static Term StringToSymbol(MachineState mx, params Term[] terms)
        {
            if (terms[0] is CharString cs)
            {
                return Symbol.Intern(cs.Value);
            }
            else
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
            }
        }

    }
}
