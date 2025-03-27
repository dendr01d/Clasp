using Clasp.Data.Metadata;
using Clasp.Data.Terms;

namespace Clasp.Ops
{
    internal static class CharacterOps
    {
        // I'm interning all my characters to begin with, so this is a little redundant...
        public static ITerm CharEq(Character c1, Character c2) => EqualityOps.Eq(c1, c2);

        public static ITerm CharLT(Character c1, Character c2) => new Boole(c1.Value < c2.Value);
        public static ITerm CharLTE(Character c1, Character c2) => new Boole(c1.Value <= c2.Value);
        public static ITerm CharGT(Character c1, Character c2) => new Boole(c1.Value > c2.Value);
        public static ITerm CharGTE(Character c1, Character c2) => new Boole(c1.Value >= c2.Value);

        public static ITerm CharacterToInteger(Character c) => new FixNum(c.Value);

        public static ITerm IntegerToCharacter(FixNum z) => new Character((char)z.Value);

    }
}
