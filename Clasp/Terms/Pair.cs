namespace Clasp
{
    internal class Pair : Expression
    {
        private Expression _car;
        private Expression _cdr;

        protected Pair(Expression car, Expression cdr)
        {
            _car = car;
            _cdr = cdr;
        }

        public override bool IsAtom => false;
        public override Expression Car => _car;
        public override Expression Cdr => _cdr;
        public override Expression SetCar(Expression expr)
        {
            _car = expr;
            return Symbol.Ok;
        }
        public override Expression SetCdr(Expression expr)
        {
            _cdr = expr;
            return Symbol.Ok;
        }


        #region Constructors

        public static Pair Cons(Expression car, Expression cdr) => new Pair(car, cdr);

        public static Expression MakeList(params Expression[] es)
        {
            return es.Length == 0
                ? Nil
                : Cons(es[0], MakeList(es[1..]));
        }

        public static Expression MakeImproperList(params Expression[] es)
        {
            return es.Length switch
            {
                0 => Nil,
                1 => es[0],
                2 => Cons(es[0], es[1]),
                _ => Cons(es[0], MakeImproperList(es[1..]))
            };
        }

        public static Expression Append(Expression ls, Expression t)
        {
            return ls.IsNil
                ? t
                : Cons(ls.Car, Append(ls.Cdr, t));
        }

        #endregion

        #region Helper Functions

        //left fold
        //not necessary unto itself, but helpful for certain arithmetic operators
        public static Expression Fold<T1, T2>(Func<T1, T2, T1> op, T1 init, Expression ls)
            where T1 : Expression
            where T2 : Expression
        {
            return ls.IsNil
                ? init
                : Fold(op, op(init, ls.Car.Expect<T2>()), ls.Cdr);
        }

        public static bool Memq(Expression obj, Expression ls)
        {
            if (ls.IsNil)
            {
                return false;
            }
            else if (ls.Car == obj) //ref equality, i.e. eq
            {
                return true;
            }
            else
            {
                return Memq(obj, ls.Cdr);
            }
        }

        public static IEnumerable<Expression> Enumerate(Expression expr)
        {
            if (expr.IsNil)
            {
                yield break;
            }
            else if (expr is Pair p)
            {
                yield return p.Car;
                foreach (Expression e in Enumerate(p.Cdr)) yield return e;
            }
            else
            {
                yield return expr;
            }
        }


        #endregion

        public string Format(char openParen, char closeParen)
        {
            if (_cdr is Pair p && p.Cdr.IsNil)
            {
                if (_car == Symbol.Quote)
                {
                    return $"'{p.Car}";
                }
                else if (_car == Symbol.Quasiquote)
                {
                    return $"`{p.Car}";
                }
                else if (_car == Symbol.Unquote)
                {
                    return $",{p.Car}";
                }
                else if (_car == Symbol.UnquoteSplicing)
                {
                    return $",@{p.Car}";
                }
            }

            return $"{openParen}{_car}{FormatTail(_cdr)}{closeParen}";
        }

        public override string ToPrinted() => Format('(', ')');
        public override string ToSerialized() => ToPrinted();

        private static string FormatTail(Expression expr)
        {
            if (expr.IsNil)
            {
                return string.Empty;
            }
            else if (expr.IsAtom)
            {
                return " . " + expr.ToString();
            }
            else
            {
                return $" {expr.Car}{FormatTail(expr.Cdr)}";
            }
        }
    }
}
