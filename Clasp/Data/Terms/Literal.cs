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
        public long AsInteger => Value.GetHashCode();

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
                if (CharacterMap.NameToChar.TryGetValue(token.Text[2..], out char value))
                {
                    return Intern(value);
                }
                throw new ClaspGeneralException(string.Format("Unknown character: '{0}'", token.Text));
            }
        }

        public override string ToString()
        {
            //if (CharacterMap.CharToName.TryGetValue(Value, out string? name))
            //{
            //    return string.Format("#\\{0}", name);
            //}
            return string.Format("#\\{0}", Value);
        }

        protected override string FormatType() => "Char";
    }

    internal sealed class CharString : Literal<string>
    {
        public CharString(string s) : base(s) { }
        public override string ToString() => string.Format("\"{0}\"", Value);

        protected override string FormatType() => "String";
    }

    internal sealed class Boolean : Literal<bool>
    {
        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);
        private Boolean(bool b) : base(b) { }
        public override string ToString() => Value ? "#t" : "#f";
        protected override string FormatType() => "Bool";
    }

    internal interface INumber
    {
        public long AsInteger { get; }
        public double AsDouble { get; }
    }

    internal sealed class Integer : Literal<long>, INumber
    {
        public static readonly Integer Zero = new Integer(0);
        public static readonly Integer One = new Integer(1);
        public static readonly Integer NegativeOne = new Integer(-1);

        public Integer(long l) : base(l) { }
        public long AsInteger => Value;
        public double AsDouble => Value;
        protected override string FormatType() => "Integer";
    }

    internal sealed class Real : Literal<double>, INumber
    {
        public Real(double d) : base(d) { }
        public long AsInteger => throw new ClaspGeneralException("Can't cast {0} value to integer.", nameof(Real));
        public double AsDouble => Value;
        protected override string FormatType() => "Real";
    }
}
