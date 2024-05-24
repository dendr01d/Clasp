namespace Clasp
{
    internal abstract class Atom : Expression
    {
        protected Atom() { }

        public override bool IsList => false;

        public override Expression Car => throw new ExpectedTypeException<Pair>(this);
        public override Expression Cdr => throw new ExpectedTypeException<Pair>(this);
        public override void SetCar(Expression expr) => throw new ExpectedTypeException<Pair>(this);
        public override void SetCdr(Expression expr) => throw new ExpectedTypeException<Pair>(this);

    }

    internal class Empty : Atom
    {
        private Empty() { }
        public static Empty Instance = new Empty();
        public override string ToString() => "()";
    }

    internal class Error : Atom
    {
        private Error() { }
        public static Error Instance = new Error();
        public override string ToString() => "#error";
    }

    internal abstract class Literal<T> : Atom
    {
        public readonly T Value;
        protected Literal(T val)
        {
            Value = val;
        }
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

        public override string ToString() => Value ? "#t" : "#f";
    }

    internal class FixNum : Literal<int>
    {
        public FixNum(int i) : base(i) { }
        public override string ToString() => Value.ToString();
    }

    internal class Number : Literal<double>
    {
        public Number(double d) : base(d) { }

        public override string ToString() => Value.ToString();
    }

    internal class Character : Literal<char>
    {
        public Character(char c) : base(c) { }
        public override string ToString() => $"\\{Value}";
    }

    //internal abstract class SVector<T> : Constant
    //{
    //    public readonly Literal<T> Contents;
    //}

    //internal abstract class SString : SVector<Character>
    //{
    //    //I don't think the contents here are typed correctly??
    //}
}
