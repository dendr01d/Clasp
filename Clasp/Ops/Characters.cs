using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Characters
    {
        public static Term CharEq(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c1 && terms[1] is Character c2)
            {
                return Helpers.AsTerm(c1.Value == c2.Value);
            }
            else
            {
                throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
            }
        }

        public static Term CharLT(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c1 && terms[1] is Character c2) return Helpers.AsTerm(c1.AsInteger < c2.AsInteger);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term CharGT(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c1 && terms[1] is Character c2) return Helpers.AsTerm(c1.AsInteger > c2.AsInteger);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term CharLTE(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c1 && terms[1] is Character c2) return Helpers.AsTerm(c1.AsInteger <= c2.AsInteger);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term CharGTE(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c1 && terms[1] is Character c2) return Helpers.AsTerm(c1.AsInteger >= c2.AsInteger);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term CharacterToInteger(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Character c) return new Integer(c.AsInteger);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

        public static Term IntegerToCharacter(MachineState mx, params Term[] terms)
        {
            if (terms[0] is Integer i) return Character.Intern((char)i.Value);
            else throw new ProcessingException.InvalidPrimitiveArgumentsException(terms[0]);
        }

    }
}
