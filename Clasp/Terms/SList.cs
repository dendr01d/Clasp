namespace Clasp
{
    internal abstract class SList : Expression
    {
        protected readonly Expression Car;
        protected readonly Expression Cdr;

        protected SList(Expression car, Expression cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public bool IsEmpty => IsFalse;
        public override bool IsAtom => false;
        public override bool IsList => true;


        public override Expression ExpectCar() => Car;
        public override Expression ExpectCdr() => Cdr;

        public Expression ExpectArg(int i)
        {
            if (i == 0 || IsEmpty)
            {
                return Car;
            }
            else if (Cdr.IsAtom && i == 1)
            {
                return Cdr;
            }
            else
            {
                return Cdr.ExpectList().ExpectArg(i - 1);
            }
        }

        public static SList ConstructLinked(Expression[] exprs)
        {
            if (exprs.Length == 0)
            {
                return Nil;
            }
            else if (exprs.Length == 1)
            {
                return Pair.Cons(exprs[0], Nil);
            }
            else
            {
                return Pair.Cons(exprs[0], ConstructLinked(exprs[1..]));
            }
        }

        public SList Map(Func<Expression, Expression> fun)
        {
            if (IsEmpty)
            {
                return Nil;
            }
            else
            {
                if (!Cdr.IsList)
                {
                    return Pair.Cons(fun(Car), fun(Cdr));
                }
                else
                {
                    return Pair.Cons(fun(Car), Cdr.ExpectList().Map(fun));
                }
            }
        }

        public override string ToString() => IsNil ? "()" : $"({Car}{FormatContents(Cdr)})";

        private static string FormatContents(Expression expr)
        {
            if (expr.IsNil)
            {
                return String.Empty;
            }
            else if (expr is Pair p)
            {
                return $" {p.Car}{FormatContents(p.Cdr)}";
            }
            else
            {
                return " " + expr.ToString();
            }
        }
    }

    internal class Empty : SList
    {
        public Empty() : base(Nil, Nil) { } //is this allowed?
        public override bool IsAtom => true;
        public override Expression ExpectCar() => Nil;
        public override Expression ExpectCdr() => Nil;
        public override Expression Evaluate(Environment env) => this;
    }

    internal class Pair : SList
    {
        protected Pair(Expression car, Expression cdr) : base(car, cdr) { }

        public static Pair Cons(Expression car, Expression cdr) => new(car, cdr);

        public override Expression Evaluate(Environment env)
        {
            Procedure op = Car.Evaluate(env).ExpectProcedure();
            SList args = Cdr.ExpectList().Map(x => x.Evaluate(env));

            return op.Apply(args, env);
        }
    }
}
