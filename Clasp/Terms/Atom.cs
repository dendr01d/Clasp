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

    internal class SpecialFormRef : Atom
    {
        public readonly Symbol Ref;

        public SpecialFormRef(Symbol spRef) => Ref = spRef;
        public override string ToPrinted() => $"{{{Ref.Name}}}";
        public override string ToSerialized() => Ref.Name;
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
        public Character(char c) : base(c) { }
        public override string ToPrinted() => $"\\{Value}";
    }

    //internal class Number : Literal<double>
    //{
    //    public Number(double d) : base(d) { }
    //}

}
