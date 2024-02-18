namespace Clasp
{
    internal abstract class Expression
    {
        protected Expression() { }

        public static readonly Symbol TrueValue = new("#t");
        public static readonly Symbol FalseValue = new("#f");
        public static readonly Empty Nil = new();

        public abstract bool IsAtom { get; }
        public abstract bool IsList { get; }
        public bool IsFalse => ReferenceEquals(this, FalseValue);
        public bool IsTrue => !IsFalse;
        public bool IsNil => ReferenceEquals(this, Nil);


        #region Type-Checking

        public virtual Symbol ExpectSymbol() => ExpectDerived<Symbol>();
        public virtual Procedure ExpectProcedure() => ExpectDerived<Procedure>();
        public virtual SList ExpectList() => ExpectDerived<SList>();

        private T ExpectDerived<T>() where T : Expression
        {
            if (this is T derived)
            {
                return derived;
            }
            else
            {
                throw new Exception($"Expected {typeof(T).Name}: {this}");
            }
        }

        public virtual Expression ExpectCar() => throw new Exception($"Tried to take car of atom: {this}");
        public virtual Expression ExpectCdr() => throw new Exception($"Tried to take cdr of atom: {this}");

        #endregion

        #region Equality Checks

        public virtual bool ValueEquals(Expression other) => other.ValueEquals(this);
        public virtual bool ValueEquals(Symbol other) => this is Symbol sym && sym.Name == other.Name;
        public virtual bool ValueEquals(Procedure other) => this is Procedure proc && ReferenceEquals(proc, other);
        public virtual bool ValueEquals(Number other) => this is Number num && num.Value == other.Value;
        public virtual bool ValueEquals(Empty other) => this is Empty;
        public virtual bool ValueEquals(Pair other) => this is Pair pair
            && pair.ExpectCar().ValueEquals(other.ExpectCar())
            && pair.ExpectCdr().ValueEquals(other.ExpectCdr());

        #endregion

        #region Default Conversions

        public static implicit operator Expression(double d) => new Number(d);
        public static implicit operator Expression(int i) => new Number(i);
        public static implicit operator Expression(char c) => new Character(c);
        public static implicit operator Expression(string s) => new Symbol(s);

        #endregion

        public abstract Expression Evaluate(Environment env);

        public abstract override string ToString();
    }
}
