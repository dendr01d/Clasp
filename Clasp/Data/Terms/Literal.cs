using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Clasp.Data.Text;

namespace Clasp.Data.Terms
{
    internal abstract class Literal<T> : Atom
    {
        public readonly T Value;
        protected Literal(T value) => Value = value;
        public override string ToString() => Value?.ToString() ?? "#?";
    }

    internal sealed class Character : Literal<char>
    {
        private Character(char c) : base(c) { }

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
            if (token.Text.Length == 3
                && token.Text[0] == '#'
                && token.Text[1] == '\\')
            {
                return Intern(token.Text[2]);
            }
            else
            {
                return token.Text[2..] switch
                {
                    "space" => Intern(' '),
                    "tab" => Intern('\t'),
                    "newline" => Intern('\n'),
                    "return" => Intern('\r'),
                    _ => throw new ClaspGeneralException(string.Format("Unknown character: '{0}'", token.Text))
                };
            }
        }

        public override string ToString()
        {
            return Value switch
            {
                ' ' => @"#\space",
                '\t' => @"#\tab",
                '\n' => @"#\newline",
                '\r' => @"#\return",
                _ => string.Format("#\\{0}", Value)
            };
        }
    }

    internal sealed class CharString : Literal<string>
    {
        public CharString(string s) : base(s) { }
        public override string ToString() => string.Format("\"{0}\"", Value);
    }

    internal sealed class Boolean : Literal<bool>
    {
        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);
        private Boolean(bool b) : base(b) { }
        public override string ToString() => Value ? "#t" : "#f";
    }

    internal sealed class Integer : Literal<long>
    {
        public Integer(long l) : base(l) { }
    }

    internal sealed class Real : Literal<double>
    {
        public Real(double d) : base(d) { }
    }
}
