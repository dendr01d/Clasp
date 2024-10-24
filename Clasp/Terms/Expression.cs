using System.Diagnostics;
using System.Text;

namespace Clasp
{
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

        /// <summary> True iff the expression is a proper nil-terminated list.</summary>
        public bool IsList => IsNil || (!IsAtom && Cdr.IsList);

        //public bool IsDottedPair => IsPair && !Cdr.IsNil && Cdr.IsAtom;
        //public bool IsDottedList => IsDottedPair || (IsPair && Cdr.IsDottedList);

        /// <summary> True iff the expression is a list with <paramref name="sym"/> in the Car position.</summary>
        public bool IsTagged(Symbol sym) => IsPair && !Cdr.IsNil && Car.Pred_Eq(sym);

        /// <summary> True iff the expression is a list of 2+ terms, and the second term is an ellipsis.</summary>
        public bool IsEllipticTerm => IsPair && Cdr.IsPair && Cadr.Pred_Eq(Symbol.Ellipsis);

        #endregion

        #region Equality Predicates

        public bool Pred_Eq(Expression other) => Pred_Eq(this, other);
        public bool Pred_Eqv(Expression other) => Pred_Eqv(this, other);
        public bool Pred_Equal(Expression other) => Pred_Equal(this, other);
        public bool Pred_FreeIdEq(Expression other) => Pred_FreeIdEq(this, other);
        public bool Pred_BoundIdEq(Expression other) => Pred_BoundIdEq(this, other);


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
                //(Vector v1, Vector v2) => v1.EnumerableData.Zip(v2.EnumerableData, (x, y) => Pred_Equal(x, y)).Aggregate((x, y) => x && y),
                (Pair p1, Pair p2) => Pred_Equal(p1.Car, p2.Car) && Pred_Equal(p1.Cdr, p2.Cdr),
                (_, _) => Pred_Eqv(e1, e2)
            };
        }

        public static bool Pred_FreeIdEq(Expression e1, Expression e2)
        {
            return Pred_Eq(e1.Resolve(), e2.Resolve());
        }

        public static bool Pred_BoundIdEq(Expression e1, Expression e2)
        {
            return Pred_FreeIdEq(e1, e2)
                && e1.GetMarks().SetEquals(e2.GetMarks());
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

        public virtual Expression Mark(params int[] marks) => this;
        public virtual HashSet<int> GetMarks() => new HashSet<int>();

        public virtual Expression Subst(Identifier id, Symbol s) => this;

        public virtual Expression Resolve() => this;
        public virtual Expression Strip() => this;
        public virtual Expression Expose() => this;

        #endregion

        /// <summary>
        /// Destructures compiled objects back to rudimentary expressions
        /// </summary>
        public abstract Expression Deconstruct();

        /// <summary>
        /// Returns a syntactically-complete string that parses to the expression
        /// </summary>
        public abstract string Serialize();

        /// <summary>
        /// Return a string that semantically represents the expression
        /// </summary>
        public abstract string Print();

        public sealed override string ToString() => Print();

    }
}
