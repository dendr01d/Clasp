using System.Diagnostics;

namespace Clasp
{
    internal abstract class Expression
    {
        protected Expression() { }

        #region Static Terms
        public static readonly Empty Nil = Empty.Instance;
        public static readonly Error Error = Error.Instance;
        #endregion

        #region Native Predicate Fields
        public abstract bool IsAtom { get; }

        public bool IsList => IsNil || (!IsAtom && Cdr.IsList);

        public bool IsNil => ReferenceEquals(this, Nil);
        public bool IsFalse => ReferenceEquals(this, Boolean.False);
        public bool IsTrue => !IsFalse;
        #endregion

        #region Structural Access
        public abstract Expression Car { get; }
        public abstract Expression Cdr { get; }
        public abstract Expression SetCar(Expression expr);
        public abstract Expression SetCdr(Expression expr);

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

        #region Equality

        public static bool Pred_Eq(Expression e1, Expression e2) => ReferenceEquals(e1, e2);
        public static bool Pred_Eqv(Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                (Number f1, Number f2) => f1.Value == f2.Value,
                (Character c1, Character c2) => c1.Value == c2.Value,
                (_, _) => Pred_Eq(e1, e2)
            };
        }
        public static bool Pred_Equal(Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                //(CString c1, CString c2) => c1.Value == c2.Value,
                (Pair p1, Pair p2) => Pred_Equal(p1.Car, p2.Car) && Pred_Equal(p1.Cdr, p2.Cdr),
                (_, _) => Pred_Eqv(e1, e2)
            };
        }

        #endregion

        public sealed override string ToString() => ToPrinted();

        /// <summary>
        /// Returns a string representing a prettier-printed form of the object
        /// </summary>
        /// <returns></returns>
        public abstract string ToPrinted();

        /// <summary>
        /// Returns a string representing a more "real" representation of the object
        /// </summary>
        public abstract string ToSerialized();
    }
}
