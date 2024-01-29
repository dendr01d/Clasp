using System;

namespace Clasp
{
    public abstract class Expression
    {
        protected abstract T Match<T>(Func<Atom, T> ifAtom, Func<ExprList, T> ifList);
        public bool IsAtomic => Match(_ => true, x => x.IsEmpty);
        public Atom Evaluate() => Match(x => x, x => x.IsEmpty ? Nil : Operations.Process(x).Evaluate());

        public static readonly Atom True = new Atom("t");

        public static readonly Atom Nil = new Atom("()");

        public class Atom : Expression
        {
            public readonly string Value;

            public Atom(string value)
            {
                Value = value;
            }

            protected override T Match<T>(Func<Atom, T> ifAtom, Func<ExprList, T> ifList) => ifAtom(this);
        }

        public class ExprList : Expression
        {
            public readonly Expression[] Elements;

            public bool IsEmpty => Elements.Length == 0;

            public Expression Car => IsEmpty ? Nil : Elements[0];

            public Expression Cdr => Elements.Length > 2 ? new ExprList(Elements[1..]) : Nil;

            protected override T Match<T>(Func<Atom, T> ifAtom, Func<ExprList, T> ifList) => ifList(this);

            public ExprList(params Expression[] elems)
            {
                Elements = elems;
            }
        }
    }

    //public class Value : Atom
    //{
    //    public string Data;
    //    public override string Evaluate() => Data;

    //    protected override T MatchAtom<T>(Func<Value, T> ifValue, Func<Operator, T> ifOperator) => ifValue(this);

    //    public Value(string data) { Data = data; }

    //    public static readonly Value True = new Value("t");
    //    public static readonly Value False = new Value("f");
    //}

    //public abstract class Operator : Atom
    //{
    //    public abstract string Name { get; }
    //    public override string Evaluate() => Name;
    //    protected override T MatchAtom<T>(Func<Value, T> ifValue, Func<Operator, T> ifOperator) => ifOperator(this);
    //    public abstract Expression Operate(params Expression[] args);

    //    public static Expression Decide(bool decision) => decision ? Value.True : ExprList.Nil;
    //}

    //public class Op_Quote : Operator
    //{
    //    public override string Name => "quote";

    //    public override Expression Operate(params Expression[] args) => args[0];
    //}

    //public class Op_Atom : Operator
    //{
    //    public override string Name => "atom";

    //    public override Expression Operate(params Expression[] args) => Decide(args[0].IsAtomic);
    //}

    //public class Op_Eq : Operator
    //{
    //    public override string Name => "eq";

    //    public override Expression Operate(params Expression[] args) => Decide(args[0].Evaluate() == args[1].Evaluate());
    //}

    //public class Op_Car : Operator
    //{
    //    public override string Name => "car";

    //    public override Expression Operate(params Expression[] args)
    //    {
    //        if (TypeCheck(args[0], out ExprList list))
    //        {
    //            return list.Car;
    //        }
    //        else
    //        {
    //            throw new ArgumentOutOfRangeException("Operation 'car' expects a non-empty list");
    //        }
    //    }
    //}

    //public class Op_Cdr : Operator
    //{
    //    public override string Name => "cdr";

    //    public override Expression Operate(params Expression[] args)
    //    {
    //        if (TypeCheck(args[0], out ExprList list))
    //        {
    //            return list.Cdr;
    //        }
    //        else
    //        {
    //            throw new ArgumentOutOfRangeException("Operation 'cdr' expects a list with at least one element");
    //        }
    //    }
    //}

    //public class Op_Cons : Operator
    //{
    //    public override string Name => "cons";

    //    public override Expression Operate(params Expression[] args) => new ExprList(args);
    //}

    //public class Op_Cond : Operator
    //{
    //    public override string Name => "cond";

    //    public override Expression Operate(params Expression[] args)
    //    {
    //        for (int i = 0; i < args.Length; ++i)
    //        {
    //            if (TypeCheck(args[i], out ExprList list))
    //            {
    //                if (list.Car.Evaluate() == Value.True.Data)
    //                {
    //                    return list.Cdr;
    //                }
    //            }
    //        }

    //        throw new ArgumentOutOfRangeException("None of the conditionals in the 'cond' arguments evaluated to true");
    //    }
    //}


    internal static class Operations
    {
        public static Expression Process(params Expression[] args)
        {
            throw new NotImplementedException();

            ExpectArgNumber(1, args);

            string opName = Head(args).Evaluate().Value;

            if (!Rest(args).Any())
            {
                //return value of op? some kind of lambda expression
            }

            Operation op = IdentifyOperator(opName);

            var result = op(args[1..]);

        }

        private delegate Tuple<Expression, Expression[]> Operation(Expression[] exprs);

        private static Operation IdentifyOperator(string op)
        {
            return op switch
            {
                "quote" => Quote,
            };
        }

        #region Primitive Operators

        private static Tuple<Expression, Expression[]> Quote(Expression[] exprs)
        {
            ExpectArgNumber(1, exprs);
            return new(Head(exprs), Rest(exprs));
        }

        private static Tuple<Expression, Expression[]> IsAtom(Expression[] exprs)
        {
            ExpectArgNumber(1, exprs);
            return new(Head(exprs).IsAtomic ? Expression.Atom.True : Expression.ExprList.Nil, Rest(exprs));
        }

        private static Tuple<Expression, Expression[]> AreEqual(Expression[] exprs)
        {
            throw new NotImplementedException();
        }

        private static Tuple<Expression, Expression[]> GetCar(Expression[] exprs)
        {
            throw new NotImplementedException();
        }

        private static Tuple<Expression, Expression[]> GetCdr(Expression[] exprs)
        {
            throw new NotImplementedException();
        }

        private static Tuple<Expression, Expression[]> Construct(Expression[] exprs)
        {
            throw new NotImplementedException();
        }

        private static Tuple<Expression, Expression[]> Conditional(Expression[] exprs)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Helpers

        private static void ExpectArgNumber(int numberOfArgs, Expression[] exprs)
        {
            if (numberOfArgs > exprs.Length)
            {
                throw new ArgumentOutOfRangeException("Operator was provided with too few arguments to be processed");
            }
        }

        private static Expression Head(Expression[] exprs) => exprs[0];

        private static Expression[] Rest(Expression[] exprs) => exprs[1..];

        private static Expression ExpectAtom(Expression expr)
        {
            if (expr.IsAtomic)
            {
                return expr;
            }

            throw new ArgumentException("Expected atomic expression as argument to operator");
        }

        private static Expression[] ExpectList(Expression expr)
        {
            if (!expr.IsAtomic)
            {
                return ((Expression.ExprList)expr).Elements;
            }

            throw new ArgumentException("Expected list expression as argument to operator");
        }

        #endregion
    }

}
