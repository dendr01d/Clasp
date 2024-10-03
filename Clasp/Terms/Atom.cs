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

        public override Expression Deconstruct() => this;
        public override string Serialize() => "'()";
        public override string Print() => "nil";
    }

    internal class Error : Atom
    {
        private readonly string _description;

        public Error(string desc) => _description = desc;
        public Error(Expression expr) => _description = expr.Print();
        public Error(Exception ex)
        {
            _description = string.Format("ERR: {0}{1}{2}",
                ex.Message,
                System.Environment.NewLine,
                ex.SimplifyStackTrace());
        }

        public override Expression Deconstruct() => Pair.MakeList(Symbol.Throw, new Charstring(_description));
        public override string Serialize() => Deconstruct().Serialize();
        public override string Print() => "ERR";
    }

    internal class Undefined : Atom
    {
        private Undefined() { }
        public static Undefined Instance = new Undefined();
        public override Expression Deconstruct() => Pair.List(Symbol.Undefined);
        public override string Serialize() => Deconstruct().Serialize();
        public override string Print() => "#undefined";
    }

    internal abstract class Literal<T> : Atom
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }

        public override Expression Deconstruct() => this;
        public override string Print() => Serialize();
    }

    internal class Boolean : Literal<bool>
    {
        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);

        private Boolean(bool b) : base(b) { }
        public static implicit operator Boolean(bool b) => b ? True : False;
        public static implicit operator bool(Boolean b) => b.IsTrue;

        public override string Serialize() => Value ? "#t" : "#f";
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

        public override string Serialize() => "#\\" + Value switch
        {
            '\t' => "tab",
            '\n' => "newline",
            ' ' => "space",
            (char)0 => "UNK",
            _ => Value
        };
    }

    internal class Charstring : Literal<string>
    {
        public Charstring(string s) : base(s) { }

        public static Charstring FromToken(Token t) => new Charstring(t.Text[1..^1]);

        public override string Serialize() => $"\"{Value}\"";
    }
}
