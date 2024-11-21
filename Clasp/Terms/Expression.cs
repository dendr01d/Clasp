using System.Diagnostics;
using System.Text;

namespace Clasp
{
    [DebuggerDisplay("{Display()}")]
    internal abstract class Expression
    {
        protected Expression() { }

        #region Static Terms
        public static readonly Empty Nil = Empty.Instance;
        #endregion

        #region Native Predicate Fields

        public abstract bool IsAtom { get; }
        public bool IsPair => !IsAtom;
        public bool IsNil => ReferenceEquals(this, Nil);
        
        public bool IsFalse => ReferenceEquals(this, Boolean.False);
        public bool IsTrue => !IsFalse;

        #endregion

        #region Equality Predicates

        public virtual bool Pred_Eq(Expression other) => false;
        public virtual bool Pred_Eqv(Expression other) => false;
        public virtual bool Pred_Equal(Expression other) => false;

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
                (Integer i1, Integer i2) => i1.Value == i2.Value,
                (Double d1, Double d2) => d1.Value == d2.Value,
                (_, _) => Pred_Eq(e1, e2)
            };
        }

        public static bool Pred_Equal(Expression e1, Expression e2)
        {
            return (e1, e2) switch
            {
                (CharString c1, CharString c2) => c1.Value == c2.Value,
                (Vector v1, Vector v2) => v1.VecEquals(v2),
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

        public T Expect<T>() where T : Expression => Expect<T>(this);

        #endregion

        #region Implicit C# Type-Casting

        public static implicit operator Expression(bool b) => b ? Boolean.True : Boolean.False;

        #endregion

        #region Assumed List-Structure Access

        public virtual Expression Car => throw new ExpectedTypeException<Pair>(this);
        public virtual Expression Cdr => throw new ExpectedTypeException<Pair>(this);

        public Expression Caar => Car.Car;
        public Expression Cadr => Cdr.Car;
        public Expression Cdar => Car.Cdr;
        public Expression Cddr => Cdr.Cdr;

        public Expression Caaar => Car.Car.Car;
        public Expression Caadr => Cdr.Car.Car;
        public Expression Cadar => Car.Cdr.Car;
        public Expression Caddr => Cdr.Cdr.Car;

        public Expression Cdaar => Car.Car.Cdr;
        public Expression Cdadr => Cdr.Car.Cdr;
        public Expression Cddar => Car.Cdr.Cdr;
        public Expression Cdddr => Cdr.Cdr.Cdr;

        public Expression Cadddr => Cdr.Cdr.Cdr.Car;

        #endregion

        #region Syntactic Analysis

        public virtual void Mark(params int[] marks) { }
        public virtual IEnumerable<int> GetMarks() => new HashSet<int>();
        public virtual void Substitute(Expression id, Symbol s) { }

        public virtual Expression Resolve() => this;
        public virtual Expression Strip() => this;
        public virtual Expression Expose() => this;

        #endregion


        /// <summary>
        /// Returns a syntactically-complete string that parses to the expression
        /// </summary>
        public abstract string Write();

        /// <summary>
        /// Return a string that semantically represents the expression
        /// </summary>
        public abstract string Display();

        public sealed override string ToString() => Display();

    }
}
