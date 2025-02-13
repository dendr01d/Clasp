using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class Characters
    {
        // I'm interning all my characters to begin with, so this is a little redundant...
        public static Term CharEq(Character c1, Character c2) => Equality.Eq(c1, c2);

        public static Term CharLT(Character c1, Character c2) => c1.AsInteger < c2.AsInteger;
        public static Term CharLTE(Character c1, Character c2) => c1.AsInteger <= c2.AsInteger;
        public static Term CharGT(Character c1, Character c2) => c1.AsInteger > c2.AsInteger;
        public static Term CharGTE(Character c1, Character c2) => c1.AsInteger >= c2.AsInteger;

        public static Term CharacterToInteger(Character c) => new Integer(c.AsInteger);

        public static Term IntegerToCharacter(IntegralNumeric z) => Character.Intern((char)z.AsInteger);

    }
}
