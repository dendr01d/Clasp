namespace Clasp
{
    internal abstract class Expression
    {
        protected Expression() { }

        #region Static Terms
        public static readonly Empty Nil = Empty.Instance;
        public static readonly Error Error = Error.Instance;
        #endregion

        #region Predicate Fields
        public abstract bool IsList { get; }
        public bool IsNil => ReferenceEquals(this, Nil);
        public bool IsAtom => !IsList && !IsNil;
        public bool IsFalse => ReferenceEquals(this, Boolean.False);
        public bool IsTrue => !IsFalse;
        public bool Is<T>() where T : Expression => this is T;
        #endregion

        #region Structural Access
        public abstract Expression Car { get; }
        public abstract Expression Cdr { get; }
        public abstract void SetCar(Expression expr);
        public abstract void SetCdr(Expression expr);

        public Expression Caar => Car.Car;
        public Expression Cadr => Cdr.Car;
        public Expression Cdar => Car.Cdr;
        public Expression Cddr => Cdr.Cdr;
        public Expression Cdddr => Cdr.Cdr.Cdr;
        public Expression Cadar => Car.Cdr.Car;
        public Expression Caddr => Cdr.Cdr.Car;
        public Expression Cadddr => Cdr.Cdr.Cdr.Car;

        #endregion

        #region Conversions

        public T Expect<T>()
            where T : Expression
        {
            return this is T output
                ? output
                : throw new ExpectedTypeException<T>(this);
        }

        //public static implicit operator Expression(double d) => new Number(d);
        //public static implicit operator Expression(int i) => new Number(i);
        //public static implicit operator Expression(char c) => new Character(c);
        //public static implicit operator Expression(string s) => new Symbol(s);

        #endregion

        public abstract override string ToString();
    }
}
