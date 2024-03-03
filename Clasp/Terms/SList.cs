namespace Clasp
{
    internal abstract record SList(Expression Car, Expression Cdr) : Expression()
    {
        public override bool IsAtom => false;
        public override bool IsList => true;

        protected override string FormatString() => IsNil ? "()" : $"({Car}{FormatContents(Cdr)})";

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
                return " . " + expr.ToString();
            }
        }

        #region Helpers

        public Expression this[int i] => AtIndex(i);

        private Expression AtIndex(int i)
        {
            if (i <= 0 || IsNil)
            {
                return Car;
            }
            else if (i == 1 && Cdr.IsAtom)
            {
                return Cdr;
            }
            else
            {
                return Cdr.The<SList>().AtIndex(i - 1);
            }
        }

        public IEnumerable<Expression> Contents()
        {
            Expression target = this;

            while (!target.IsAtom)
            {
                yield return target.GetCar();
                target = target.GetCdr();
            }

            if (!target.IsNil) yield return target;
        }

        public static SList Proper(IEnumerable<Expression> exprs)
        {
            return exprs.Reverse().Aggregate(Nil.The<SList>(), (a, b) => new Pair(b, a));
        }

        public static Expression Improper(IEnumerable<Expression> exprs)
        {
            return exprs.Reverse().Aggregate((a, b) => new Pair(b, a));
        }

        public SList EvLis(Environment env) => Proper(Contents().Select(x => x.CallEval(env)));
        public SList QEvList(Environment env) => Proper(Contents().Select(x => QuasiEval(x, env)));

        private static Expression QuasiEval(Expression expr, Environment env)
        {
            if (expr.IsA<SList>() && expr.The<SList>().Car is SPUnQuote)
            {
                return expr.CallEval(env);
            }
            else
            {
                return expr;
            }
        }

        #endregion
    }

    internal sealed record Empty() : SList(Nil, Nil)
    {
        public override bool IsAtom => true;
        protected override Recurrence Evaluate(Environment env) => FinishedEval(Nil);
    }

    internal record Pair(Expression Car, Expression Cdr) : SList(Car, Cdr)
    {
        protected override Recurrence Evaluate(Environment env)
        {
            Operator op = Car.CallEval(env).The<Operator>();

            if (op.IsA<SpecialForm>())
            {
                SList args = Cdr.The<SList>();
                return op.Apply(args, env);
            }
            else
            {
                SList args = Cdr.The<SList>().EvLis(env);
                return op.Apply(args, env);
            }
        }
    }

    internal static class SListExtensions
    {
        public static IEnumerable<Expression> Select(this SList list) => Select(list, x => x);
        public static IEnumerable<T> Select<T>(this SList list, Func<Expression, T> op) => list.Contents().Select(op);

        public static IEnumerable<double> AsNumbers(this SList list) => list.Select(x => x.The<Number>().Value);
        public static T Aggregate<T>(this SList list, Func<T, T, T> op)
            where T : Expression => list.Select(x => x.The<T>()).Aggregate(op);
        public static TO Aggregate<T, TO>(this SList list, TO seed, Func<TO, T, TO> op)
            where T : Expression => list.Select(x => x.The<T>()).Aggregate(seed, op);
    }
}
