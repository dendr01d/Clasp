namespace Clasp
{
    internal abstract class SList : Expression
    {
        protected readonly Expression CarField;
        protected readonly Expression CdrField;

        protected SList(Expression car, Expression cdr)
        {
            CarField = car;
            CdrField = cdr;
        }

        public bool IsEmpty => IsNil;
        public bool IsDotted => !CdrField.IsList;
        public bool IsAnteNil => CdrField.IsNil;

        public override bool IsAtom => false;
        public override bool IsList => true;


        public override Expression Car() => CarField;
        public override Expression Cdr() => CdrField;


        public Expression AtIndex(int i)
        {
            if (i == 0 || IsEmpty)
            {
                return Car();
            }
            else if (IsDotted && i == 1)
            {
                return Cdr();
            }
            else
            {
                return CdrField.AsList().AtIndex(i - 1);
            }
        }

        public Expression FromIndex(int i)
        {
            if (i == 0)
            {
                return this;
            }
            else
            {
                return Cdr().AsList().FromIndex(i - 1);
            }
        }

        public static SList ConstructLinked(params Expression[] exprs)
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
                return Pair.Cons(exprs.First(), ConstructLinked(exprs[1..]));
            }
        }

        public static Expression ConstructDotted(params Expression[] exprs)
        {
            if (exprs.Length == 0)
            {
                return Nil;
            }
            else if (exprs.Length == 1)
            {
                return exprs[0];
            }
            else
            {
                return Pair.Cons(exprs.First(), ConstructDotted(exprs[1..]));
            }
        }

        public override string ToString() => IsNil ? "()" : $"({CarField}{FormatContents(CdrField)})";

        private static string FormatContents(Expression expr)
        {
            if (expr.IsNil)
            {
                return String.Empty;
            }
            else if (expr is Pair p)
            {
                return $" {p.CarField}{FormatContents(p.CdrField)}";
            }
            else
            {
                return " . " + expr.ToString();
            }
        }

        public override string ToStringent()
        {
            if (IsEmpty)
            {
                return "()";
            }
            else
            {
                return $"({CarField.ToStringent()} . {CdrField.ToStringent()})";
            }
        }

        public override bool EqualsByValue(Expression? other)
        {
            return other is Pair p
                && p.CarField.EqualsByValue(CarField)
                && p.CdrField.EqualsByValue(CdrField);
        }
    }

    internal class Empty : SList
    {
        public Empty() : base(Nil, Nil) { } //is this allowed?
        public override bool IsAtom => true;
        public override Expression Car() => Nil;
        public override Expression Cdr() => Nil;
        public override Expression Evaluate(Environment env) => this;

        public override bool EqualsByValue(Expression? other) => other is Empty;
    }

    internal class Pair : SList
    {
        protected Pair(Expression car, Expression cdr) : base(car, cdr) { }

        public static SList Cons(Expression car, Expression cdr)
        {
            return (car.IsNil && cdr.IsNil)
                ? Nil
                : new Pair(car, cdr);
        }

        public override Expression Evaluate(Environment env)
        {
            Procedure op = CarField.Evaluate(env).AsProc();
            SList args = CdrField.AsList().EvLis(env);

            return op.Apply(args, env);
        }
    }

    internal static class SListExtensions
    {
        /// <summary>
        /// Transform a list by evaluating all of its elements, without evaluating the list itself
        /// </summary>
        public static SList EvLis(this SList list, Environment env)
        {
            if (list.IsEmpty) return list;

            Expression car = list.Car().Evaluate(env);
            if (list.Cdr().IsList)
            {
                return Pair.Cons(car, list.Cdr().AsList().EvLis(env));
            }
            else
            {
                return Pair.Cons(car, list.Cdr().Evaluate(env));
            }
        }

        public static IEnumerable<Expression> Select(this SList list) => Select(list, x => x);
        public static IEnumerable<T> Select<T>(this SList list, Func<Expression, T> op)
        {
            SList target = list;

            while (!target.IsEmpty)
            {
                yield return op.Invoke(target.Car());

                if (target.IsDotted)
                {
                    yield return op.Invoke(target.Cdr());
                    yield break;
                }
                else
                {
                    target = target.Cdr().AsList();
                }
            }
        }

        public static IEnumerable<double> AsNumbers(this SList list) => list.Select(x => x.AsNumber().Value);
        public static double AggregateNumbers(this SList list, Func<double, double, double> op) => list.AsNumbers().Aggregate(op);
    }
}
