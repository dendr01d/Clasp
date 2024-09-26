namespace Clasp
{
    internal class Pair : Expression
    {
        private Expression _car;
        private Expression _cdr;

        private Pair(Expression car, Expression cdr)
        {
            _car = car;
            _cdr = cdr;
        }

        public override bool IsAtom => false;

        public override Expression Car => _car;
        public override Expression Cdr => _cdr;

        public void SetCar(Expression car) => _car = car;
        public void SetCdr(Expression cdr) => _cdr = cdr;

        #region Constructors

        public static Pair Cons(Expression car, Expression cdr) => new Pair(car, cdr);

        public static Expression List(params Expression[] es)
        {
            return es.Length == 0
                ? Nil
                : Cons(es[0], List(es[1..]));
        }

        public static Expression ListStar(params Expression[] es)
        {
            return es.Length switch
            {
                0 => Nil,
                1 => es[0],
                2 => Cons(es[0], es[1]),
                _ => Cons(es[0], ListStar(es[1..]))
            };
        }

        public static Expression Append(Expression ls, Expression t, bool raw = true)
        {
            if (raw)
            {
                return ls.IsNil
                    ? t
                    : Cons(ls.Car, Append(ls.Cdr, t, true));
            }
            else
            {
                return Append(ls, Cons(t, Nil), true);
            }
        }

        #endregion

        #region Display

        public override Expression Deconstruct() => Cons(_car.Deconstruct(), _cdr.Deconstruct());
        public override string Serialize() => FormatPair(this, true);
        public override string Print() => FormatPair(this, false);

        private static string FormatPair(Pair p, bool asSyntax)
        {
            if (p._cdr is Pair p2 && p2.Cdr.IsNil)
            {
                if (p._car == Symbol.Quote)
                {
                    return string.Format("'{0}", asSyntax ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.Quasiquote)
                {
                    return string.Format("`{0}", asSyntax ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.Unquote)
                {
                    return string.Format(",{0}", asSyntax ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.UnquoteSplicing)
                {
                    return string.Format(",@{0}", asSyntax ? p2._car.Serialize() : p2._car.Print());
                }
            }

            return string.Format("({0}{1})",
                asSyntax ? p._car.Serialize() : p._car.Print(),
                FormatTail(p._cdr, asSyntax));
        }

        private static string FormatTail(Expression expr, bool asSyntax)
        {
            if (expr.IsNil)
            {
                return string.Empty;
            }
            else if (expr is Pair p)
            {
                return string.Format(" {0}{1}",
                    asSyntax ? p._car.Serialize() : p._car.Print(),
                    FormatTail(p._cdr, asSyntax));
            }
            else
            {
                return string.Format(" . {0}", asSyntax ? expr.Serialize() : expr.Print());
            }
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

        /// <summary>
        /// Iteratively enumerate the elements of the list front-to-back
        /// </summary>
        public static IEnumerable<Expression> Enumerate(Expression expr)
        {
            Expression target = expr;

            while (!target.IsNil)
            {
                if (target.IsAtom)
                {
                    throw new ExpectedTypeException<Pair>(target);
                }

                yield return target.Car;
                target = target.Cdr;
            }
            
            yield break;
        }

        #endregion
    }
}
