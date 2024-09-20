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

        #region Equality Predicates

        public static bool Pred_Eq(Expression e1, Expression e2)
        {
            return ReferenceEquals(e1, e2);
        }

        public static bool Pred_Eqv(Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                (Boolean b1, Boolean b2) => b1.Value == b2.Value,
                (SimpleNum pn1, SimpleNum pn2) => pn1.Value == pn2.Value,
                (Character c1, Character c2) => c1.Value == c2.Value,
                (_, _) => Pred_Eq(e1, e2)
            };
        }

        public static bool Pred_Equal(Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                (Charstring c1, Charstring c2) => c1.Value == c2.Value,
                (Vector v1, Vector v2) => v1.EnumerableData.Zip(v2.EnumerableData, (x, y) => Pred_Equal(x, y)).Aggregate((x, y) => x && y),
                (Pair p1, Pair p2) => Pred_Equal(p1.Car, p2.Car) && Pred_Equal(p1.Cdr, p2.Cdr),
                (_, _) => Pred_Eqv(e1, e2)
            };
        }

        #endregion

        #region Type-Checking

        public static T Expect<T>(Expression expr)
            where T : Expression
        {
            if (expr is T typedExpr)
            {
                return typedExpr;
            }

            throw new ExpectedTypeException<T>(expr);
        }

        public T Expect<T>()
            where T : Expression
            => Expect<T>(this);

        #endregion

        public sealed override string ToString() => ToPrinted();

        /// <summary>
        /// Returns a pretty-printed string form of the expression
        /// </summary>
        public abstract string ToPrinted();

        /// <summary>
        /// Returns a syntactic representation of the expression,
        /// such that it could be parsed back into the object
        /// </summary>
        public abstract string ToSerialized();
    }
}
