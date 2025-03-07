using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Text;
using Clasp.Exceptions;

namespace Clasp.Data.Terms
{
    internal sealed class Character : Term
    {
        public readonly char Value;

        private Character(char c) => Value = c;
        public long AsInteger => Value;

        private static readonly Dictionary<char, Character> _internment = new Dictionary<char, Character>();

        public static Character Intern(char c)
        {
            if (!_internment.ContainsKey(c))
            {
                _internment[c] = new Character(c);
            }
            return _internment[c];
        }

        public static Character Intern(Token token)
        {
            if (token.Text[0] == '#'
                && token.Text[1] == '\\')
            {
                if (token.Text.Length == 3)
                {
                    return Intern(token.Text[2]);
                }
                else if (CharacterMap.NameToChar.TryGetValue(token.Text[2..], out char value))
                {
                    return Intern(value);
                }
            }

            throw new ClaspGeneralException(string.Format("Unknown character literal: '{0}'", token.Text));
        }

        public override string ToString()
        {
            if (CharacterMap.CharToName.TryGetValue(Value, out string? name))
            {
                return string.Format("#\\{0}", name);
            }
            return string.Format("#\\{0}", Value);
        }

        public override string ToTermString() => Value.ToString();
        protected override string FormatType() => "Char";
    }
}
