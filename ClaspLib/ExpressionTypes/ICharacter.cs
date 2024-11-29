using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClaspLib
{
    public interface ICharacter : ILiteral<char> { }

    public static class CharacterExtensions
    {
        public static bool CharEq(this ICharacter c, ICharacter other) => c.Value == other.Value;
        public static bool CharLt(this ICharacter c, ICharacter other) => c.Value < other.Value;
        public static bool CharGt(this ICharacter c, ICharacter other) => c.Value > other.Value;
        public static bool CharLeq(this ICharacter c, ICharacter other) => c.Value <= other.Value;
        public static bool CharGeq(this ICharacter c, ICharacter other) => c.Value >= other.Value;
    }

}
