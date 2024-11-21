namespace Clasp
{
    internal abstract class Atom : Expression
    {
        protected Atom() { }

        public override bool IsAtom => true;
    }

    internal class Empty : Atom
    {
        private Empty() { }
        public static Empty Instance = new Empty();

        public override string Write() => "'()";
        public override string Display() => "nil";
    }

    internal class Undefined : Atom
    {
        private Undefined() { }
        public static Undefined Instance = new Undefined();

        public override string Write() => "(#undefined)";
        public override string Display() => "#undefined";
    }

    internal class Error : Atom
    {
        private readonly string _description;

        public Error(string desc) => _description = desc;
        public Error(Expression expr) => _description = expr.Write();
        public Error(Exception ex)
        {
            _description = string.Format("{0}{1}{2}",
                ex.Message,
                System.Environment.NewLine,
                ex.SimplifyStackTrace());
        }

        public override string Write() => $"(error \"{_description}\")";
        public override string Display() => $"ERROR: {_description}";
    }

    internal abstract class Literal<T> : Atom
        where T : struct
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }

        public override string Display() => Write();
    }

    internal class Boolean : Literal<bool>
    {
        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);

        private Boolean(bool b) : base(b) { }

        public static implicit operator Boolean(bool b) => b ? True : False;
        public override string Write() => Value ? "#t" : "#f";
    }

    internal class Character : Literal<char>
    {
        private static readonly Character Unknown = new Character((char)0);
        private static readonly Character Tab = new Character('\t');
        private static readonly Character Space = new Character(' ');
        private static readonly Character NewLine = new Character('\n');

        public Character(char c) : base(c) { }

        public static Character FromToken(Token t)
        {
            if (t.Text.Length == 3)
            {
                return new Character(t.Text[2]);
            }
            else
            {
                return t.Text[2..] switch
                {
                    "tab" => Tab,
                    "newline" => NewLine,
                    "space" => Space,
                    _ => Unknown
                };
            }
        }

        public override string Write() => "#\\" + Value switch
        {
            '\t' => "tab",
            '\n' => "newline",
            ' ' => "space",
            (char)0 => "UNK",
            _ => Value
        };
    }

    internal class Integer : Literal<long>
    {
        public Integer(long i) : base(i) { }

        public override string Write() => Value.ToString();
    }

    internal class Double : Literal<double>
    {
        public Double(double d) : base(d) { }

        public override string Write() => Value.ToString();
    }

    internal class CharString : Atom
    {
        public readonly string Value;

        public CharString(string value) => Value = value;

        public override string Write() => $"\"{Value}\"";

        public override string Display() => Value;
    }
}
