namespace Clasp
{
    internal abstract class Atom : Expression
    {
        protected Atom() { }

        public override bool IsAtom => true;

        public override Expression Car => throw new ExpectedTypeException<Pair>(this);
        public override Expression Cdr => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCar(Expression expr) => throw new ExpectedTypeException<Pair>(this);
        public override Expression SetCdr(Expression expr) => throw new ExpectedTypeException<Pair>(this);

    }

    internal class Empty : Atom
    {
        private Empty() { }
        public static Empty Instance = new Empty();
        public override string ToPrinted() => "()";
        public override string ToSerialized() => "'()";
    }

    internal class Error : Atom
    {
        private Error() { }
        public static Error Instance = new Error();
        public override string ToPrinted() => "#error";
        public override string ToSerialized() => "(error)";
    }

    internal abstract class Literal<T> : Atom
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }

        public override string ToPrinted() => Value?.ToString() ?? "ERR";
        public override string ToSerialized() => ToPrinted(); //literals are self-evaluating
    }

    internal class Boolean : Literal<bool>
    {
        public static readonly Boolean True = new Boolean(true);
        public static readonly Boolean False = new Boolean(false);

        private Boolean(bool b) : base(b) { }

        public static Boolean Judge(Expression expr) => expr.IsTrue ? True : False;
        public static Boolean Judge(bool b) => b ? True : False;

        public static Boolean Not(bool b) => Judge(!b);
        public static Boolean Not(Expression expr) => Not(expr.IsTrue);

        public override string ToPrinted() => Value ? "#t" : "#f";
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

        public override string ToPrinted() => Value.ToString();

        public override string ToSerialized() => "#\\" + Value switch
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

        public override string ToPrinted() => Value;
        public override string ToSerialized() => $"\"{Value}\"";
    }
}
