namespace Clasp
{
    internal static class StdOps
    {
        #region Arithmetic Operators

        public static Procedure Add = new("+", _add);
        private static Expression _add(SList l)
        {
            if (l.IsNil) return 0;
            else if (l.IsAnteNil) return l.Car().AsNumber();
            else return l.Select(x => x.AsNumber().Value).Sum();
        }

        public static Procedure Subtract = new("-", _subtract);
        private static Expression _subtract(SList l)
        {
            if (l.IsAnteNil) return l.Car().AsNumber().Value * -1;
            else return l.AggregateNumbers((a, b) => a - b);
        }

        public static Procedure Multiply = new("*", _multiply);
        private static Expression _multiply(SList l)
        {
            if (l.IsNil) return 1;
            else if (l.IsAnteNil) return l.Car().AsNumber();
            else return l.AggregateNumbers((a, b) => a * b);
        }

        public static Procedure Divide = new("/", _divide);
        private static Expression _divide(SList l)
        {
            if (l.IsAnteNil) return 1 / l.Car().AsNumber().Value;
            else return l.AggregateNumbers((a, b) => a / b);
        }

        public static Procedure Modulo = new("%", l => l.AggregateNumbers((a, b) => a % b));

        public static Procedure Expt = new("expt", l => l.AggregateNumbers(Math.Pow));
        public static Procedure AbsoluteValue = new("abs", l => Math.Abs(l.AtIndex(0).AsNumber().Value));

        public static Procedure Add1 = new("1+", l => l.AtIndex(0).AsNumber().Value + 1);
        public static Procedure Dec1 = new("1-", l => l.AtIndex(0).AsNumber().Value - 1);

        #endregion


        #region Numerical Comparison Operators

        public static Procedure NumEqual = new("=", BinaryComp((a, b) => a == b));
        public static Procedure NumGreater = new(">", BinaryComp((a, b) => a > b));
        public static Procedure NumLesser = new("<", BinaryComp((a, b) => a < b));
        public static Procedure NumGEq = new(">=", BinaryComp((a, b) => a >= b));
        public static Procedure NumLEq = new("<=", BinaryComp((a, b) => a <= b));
        public static Procedure NumNotEqual = new("/=", l => l.AsNumbers().Distinct());

        public static Procedure NumMax = new("max", BinaryOp(double.Max));
        public static Procedure NumMin = new("min", BinaryOp(double.Min));

        #endregion


        #region Logical Arithmetic

        public static Procedure And = new("and", BinaryLogic((a, b) => a && b));
        public static Procedure Or = new("or", BinaryLogic((a, b) => a || b));
        public static Procedure Xor = new("xor", BinaryLogic((a, b) => a ^ b));
        public static Procedure Not = new("not", l => l.Car().IsFalse);

        #endregion


        #region Type-Predicates

        public static Procedure IsAtom = new("atom?", l => l.Car().IsAtom);
        public static Procedure IsSymbol = new("symbol?", l => l.Car().IsSymbol);
        public static Procedure IsProcedure = new("procedure?", l => l.Car().IsProcedure);
        public static Procedure IsNumber = new("number?", l => l.Car().IsNumber);
        public static Procedure IsList = new("list?", l => l.Car().IsList);
        public static Procedure IsNil = new("null?", l => l.Car().IsNil);
        public static Procedure IsPair = new("pair?", l => l.Car().IsPair);

        public static Procedure Eqv = new("eqv?", l => _equivalent(l.AtIndex(0), l.AtIndex(1)));
        private static Expression _equivalent(Expression a, Expression b)
        {
            if (a.IsAtom && b.IsAtom)
            {
                return a.EqualsByValue(b);
            }
            else
            {
                return ReferenceEquals(a, b);
            }
        }

        public static Procedure Equal = new("equal?", l => l.AtIndex(0).EqualsByValue(l.AtIndex(1)));

        #endregion


        #region Lisp Operations

        public static Procedure Eval = new("eval", (l, e) => l.EvLis(e));
        public static Procedure Apply = new("apply", (l, e) => l.AtIndex(0).AsProc().Apply(l.AtIndex(1).AsList().EvLis(e), e));

        public static Procedure Begin = new("begin", l => l.Select().Last());

        public static Procedure List = new("list", l => l);
        public static Procedure ListStar = new Procedure("list*", l => SList.ConstructDotted(l.Select().ToArray()));

        public static Procedure Length = new("length", l => l.AtIndex(0).AsList().Select().Count());
        public static Procedure Append = new("append", l => l.Select().Aggregate(_append));
        private static Expression _append(Expression a, Expression b)
        {
            if (a.IsNil)
            {
                return b;
            }
            if (a.IsAtom)
            {
                return Pair.Cons(a, b);
            }
            else if (a.Cdr().IsNil)
            {
                return Pair.Cons(a.Car(), b);
            }
            else
            {
                return Pair.Cons(a.Car(), _append(a.Cdr(), b));
            }
        }

        public static Procedure Map = new("map", (l, e) => _map(l, e));
        private static Expression _map(SList l, Environment e)
        {
            Procedure proc = l.AtIndex(0).Evaluate(e).AsProc();
            return SList.ConstructLinked(l.AtIndex(2).AsList().Select(x => proc.Apply(x, e)).ToArray());
        }

        #endregion


        #region I/O



        #endregion


        #region Car/Cdr Extensions

        public static Procedure Caar  = new("caar", l => l.Car().Car());
        public static Procedure Cadr  = new("cadr", l => l.Car().Cdr());
        public static Procedure Cdar  = new("cdar", l => l.Cdr().Car());
        public static Procedure Cddr  = new("cddr", l => l.Cdr().Cdr());

        public static Procedure Caaar = new("caaar", l => l.Car().Car().Car());
        public static Procedure Caadr = new("caadr", l => l.Car().Car().Cdr());
        public static Procedure Cadar = new("cadar", l => l.Car().Cdr().Car());
        public static Procedure Caddr = new("caddr", l => l.Car().Cdr().Cdr());

        public static Procedure Cdaar = new("cdaar", l => l.Cdr().Car().Car());
        public static Procedure Cdadr = new("cdadr", l => l.Cdr().Car().Cdr());
        public static Procedure Cddar = new("cddar", l => l.Cdr().Cdr().Car());
        public static Procedure Cdddr = new("cdddr", l => l.Cdr().Cdr().Cdr());

        #endregion


        #region Helpers

        private static Func<SList, Expression> BinaryOp(Func<double, double, double> op)
        {
            return l => op.Invoke(l.AtIndex(0).AsNumber().Value, l.AtIndex(1).AsNumber().Value);
        }

        private static Func<SList, Expression> BinaryComp(Func<double, double, bool> op)
        {
            return l => op.Invoke(l.AtIndex(0).AsNumber().Value, l.AtIndex(1).AsNumber().Value);
        }

        private static Func<SList, Expression> BinaryLogic(Func<bool, bool, bool> op)
        {
            return l => op.Invoke(l.AtIndex(0).IsTrue, l.AtIndex(1).IsTrue);
        }

        #endregion
    }
}
