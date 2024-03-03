namespace Clasp
{
    internal static class StdOps
    {
        #region Arithmetic Operators

        public static Procedure Add = new("+", _add);
        private static Expression _add(SList l, Environment a)
        {
            return l.AsNumbers().Sum();
        }

        public static Procedure Subtract = new("-", _subtract);
        private static Expression _subtract(SList l, Environment a)
        {
            if (l.Cdr.IsNil) return l.Car.The<Number>().Value * -1;
            else return l.AsNumbers().Aggregate((a, b) => a - b);
        }

        public static Procedure Multiply = new("*", _multiply);
        private static Expression _multiply(SList l, Environment a)
        {
            if (l.IsNil) return 1;
            else return l.Aggregate<Number, double>(1.0, (a, b) => a * b.The<Number>().Value);
        }

        public static Procedure Divide = new("/", _divide);
        private static Expression _divide(SList l, Environment a)
        {
            if (l.Cdr.IsNil) return 1 / l.Car.The<Number>().Value;
            else return l.AsNumbers().Aggregate((a, b) => a / b);
        }

        public static Procedure Modulo = new("%", (l, a) => l.AsNumbers().Aggregate((a, b) => a % b));

        public static Procedure Expt = new("expt", (l, a) => l.AsNumbers().Aggregate(Math.Pow));
        public static Procedure AbsoluteValue = new("abs", (l, a) => Math.Abs(l[0].The<Number>().Value));

        public static Procedure Add1 = new("1+", (l, a) => l[0].The<Number>().Value + 1);
        public static Procedure Dec1 = new("1-", (l, a) => l[0].The<Number>().Value - 1);

        #endregion


        #region (Numerical) Comparison Operators

        public static Procedure NumEqual = new("=", (l, a) => l[0] == l[1]);
        public static Procedure NumNotEqual = new("/=", (l, a) => l[0] != l[1]);

        public static Procedure NumGreater = new(">", BinaryComp((a, b) => a > b));
        public static Procedure NumLesser = new("<", BinaryComp((a, b) => a < b));
        public static Procedure NumGEq = new(">=", BinaryComp((a, b) => a >= b));
        public static Procedure NumLEq = new("<=", BinaryComp((a, b) => a <= b));

        public static Procedure NumMax = new("max", BinaryOp(double.Max));
        public static Procedure NumMin = new("min", BinaryOp(double.Min));

        #endregion


        #region Logical Arithmetic

        //public static Procedure And = new("and", BinaryLogic((a, b) => a && b));
        //public static Procedure Or = new("or", BinaryLogic((a, b) => a || b));
        public static Procedure Xor = new("xor", BinaryLogic((a, b) => a ^ b));
        public static Procedure Not = new("not", (l, a) => l.Car.IsFalse);

        #endregion


        #region Type-Predicates

        public static Procedure IsAtom = new("atom?", (l, a) => l.Car.IsAtom);
        public static Procedure IsSymbol = new("symbol?", (l, a) => l.Car.IsA<Symbol>());
        public static Procedure IsProcedure = new("procedure?", (l, a) => l.Car.IsA<Operator>());
        public static Procedure IsNumber = new("number?", (l, a) => l.Car.IsA<Number>());
        public static Procedure IsList = new("list?", (l, a) => l.Car.IsList);
        public static Procedure IsNil = new("null?", (l, a) => l.Car.IsNil);
        public static Procedure IsPair = new("pair?", (l, a) => l.Car.IsA<Pair>());

        public static Procedure Equal = new("equal?", (l, a) => l[0] == l[1]);

        #endregion


        #region Lisp Operations

        public static Procedure Eval = new("eval", (l, e) => l.EvLis(e));
        public static Procedure Apply = new("apply", (l, a) => l.CallEval(a));

        public static Procedure List = new("list", (l, a) => l);
        public static Procedure ListStar = new Procedure("list*", (l, a) => SList.Improper(l.Contents()));

        public static Procedure Length = new("length", (l, a) => l[0].The<SList>().Contents().Count());
        public static Procedure Append = new("append", (l, a) => l.Select().Aggregate(_append));
        private static Expression _append(Expression a, Expression b)
        {
            if (a.IsNil)
            {
                return b;
            }
            if (a.IsAtom) //improper
            {
                return new Pair(a, b);
            }
            else //a is non-empty list
            {
                return new Pair(a.GetCar(), _append(a.GetCdr(), b));
            }
        }

        public static Procedure Map = new("map", (l, e) => _map(l, e));
        private static Expression _map(SList l, Environment e)
        {
            Operator op = l[0].The<Operator>();
            return SList.Proper(l.Cdr.The<SList>().Select(x => new Pair(op, x).CallEval(e)));
        }

        #endregion


        #region I/O



        #endregion


        #region Car/Cdr Extensions

        public static Procedure Caar  = new("caar", (l, a) => l.GetCar().GetCar());
        public static Procedure Cadr  = new("cadr", (l, a) => l.GetCar().GetCdr());
        public static Procedure Cdar  = new("cdar", (l, a) => l.GetCdr().GetCar());
        public static Procedure Cddr  = new("cddr", (l, a) => l.GetCdr().GetCdr());

        public static Procedure Caaar = new("caaar", (l, a) => l.GetCar().GetCar().GetCar());
        public static Procedure Caadr = new("caadr", (l, a) => l.GetCar().GetCar().GetCdr());
        public static Procedure Cadar = new("cadar", (l, a) => l.GetCar().GetCdr().GetCar());
        public static Procedure Caddr = new("caddr", (l, a) => l.GetCar().GetCdr().GetCdr());

        public static Procedure Cdaar = new("cdaar", (l, a) => l.GetCdr().GetCar().GetCar());
        public static Procedure Cdadr = new("cdadr", (l, a) => l.GetCdr().GetCar().GetCdr());
        public static Procedure Cddar = new("cddar", (l, a) => l.GetCdr().GetCdr().GetCar());
        public static Procedure Cdddr = new("cdddr", (l, a) => l.GetCdr().GetCdr().GetCdr());

        #endregion


        #region Helpers

        private static Func<SList, Environment, Expression> BinaryOp(Func<double, double, double> op)
        {
            return (l, a) => op.Invoke(l[0].The<Number>().Value, l[1].The<Number>().Value);
        }

        private static Func<SList, Environment, Expression> BinaryComp(Func<double, double, bool> op)
        {
            return (l, a) => op.Invoke(l[0].The<Number>().Value, l[1].The<Number>().Value);
        }

        private static Func<SList, Environment, Expression> BinaryLogic(Func<bool, bool, bool> op)
        {
            return (l, a) => op.Invoke(l[0].IsTrue, l[1].IsTrue);
        }

        #endregion
    }
}
