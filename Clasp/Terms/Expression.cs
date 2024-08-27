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

    internal static class Expression_Extensions
    {
        #region Equality Predicates

        public static bool Eq(this Expression e1, Expression e2)
        {
            return ReferenceEquals(e1, e2);
        }

        public static bool Eqv(this Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                (Boolean b1, Boolean b2) => b1.Value == b2.Value,
                (SimpleNum pn1, SimpleNum pn2) => pn1.Value == pn2.Value, 
                (Character c1, Character c2) => c1.Value == c2.Value,
                (_, _) => Eq(e1, e2)
            };
        }

        public static bool Equal(this Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                //(CString c1, CString c2) => c1.Value == c2.Value,
                (Pair p1, Pair p2) => Equal(p1.Car, p2.Car) && Equal(p1.Cdr, p2.Cdr),
                (_, _) => Eqv(e1, e2)
            };
        }

        #endregion

        #region Type-Checking

        public static T Expect<T>(this Expression expr)
            where T : Expression
        {
            if (expr is T typedExpr)
            {
                return typedExpr;
            }

            throw new ExpectedTypeException<T>(expr);
        }

        #endregion
    }
}
