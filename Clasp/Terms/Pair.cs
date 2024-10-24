using System.Collections;

namespace Clasp
{
    internal class Pair : Expression, IEnumerable<Expression>
    {
        protected Expression _car { get; private set; }
        protected Expression _cdr { get; private set; }

        protected Pair(Expression car, Expression cdr)
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

        /// <summary>
        /// Conditionally <see cref="Cons(Expression, Expression)"/> the given elements together, but
        /// only if the result would be structurally distinct from the existing Pair.
        /// </summary>
        public Pair ConsIfNew(Expression car, Expression cdr)
        {
            if (!_car.Pred_Eq(car) || !_cdr.Pred_Eq(cdr))
            {
                return Cons(car, cdr);
            }

            return this;
        }

        public static Expression List(params Expression[] es)
        {
            return es.Length == 0
                ? Nil
                : Cons(es[0], List(es[1..]));
        }

        public static Expression ListStar(Expression e, params Expression[] es)
        {
            return es.Length == 0
                ? e
                : Cons(e, ListStar(es[0], es[1..]));
        }

        /// <summary>
        /// Assuming <paramref name="ls"/> is a proper list, returns a new list where its
        /// nil-terminator is replaced by <paramref name="t"/>.
        /// </summary>
        public static Expression Append(Expression ls, Expression t)
        {
            return ls.IsNil
                ? t
                : Cons(ls.Car, Append(ls.Cdr, t));
        }

        /// <summary>
        /// Assuming <paramref name="ls"/> is a proper list, returns a new proper list where
        /// the last item before the nil-terminator is <paramref name="t"/>.
        /// </summary>
        public static Pair AppendLast(Expression ls, Expression t) => Append(ls, Cons(t, Nil)).Expect<Pair>();

        /// <summary>
        /// Equivalent to calling <see cref="ListStar(Expression[])"/> on <paramref name="maybeLs"/>
        /// (or the elements thereof) and <paramref name="t"/>.
        /// </summary>
        public static Expression AppendStar(Expression maybeLs, Expression t) => maybeLs switch
        {
            Empty => t,
            Pair p => Cons(p.Car, AppendStar(p.Cdr, t)),
            _ => Cons(maybeLs, t)
        };

        #endregion

        #region Display

        public override Expression Deconstruct() => Cons(_car.Deconstruct(), _cdr.Deconstruct());
        public override string Serialize() => FormatPair(this, true);
        public override string Print() => FormatPair(this, false);

        private static string FormatPair(Pair p, bool serial)
        {
            if (p._cdr is Pair p2 && p2.Cdr.IsNil)
            {
                if (p._car == Symbol.Quote)
                {
                    return string.Format("'{0}", serial ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.Quasiquote)
                {
                    return string.Format("`{0}", serial ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.Unquote)
                {
                    return string.Format(",{0}", serial ? p2._car.Serialize() : p2._car.Print());
                }
                else if (p._car == Symbol.UnquoteSplicing)
                {
                    return string.Format(",@{0}", serial ? p2._car.Serialize() : p2._car.Print());
                }
            }

            return string.Format("({0}{1})",
                serial ? p._car.Serialize() : p._car.Print(),
                FormatTail(p._cdr, serial));
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

        #region IEnumerable

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

        public static IEnumerable<T> Enumerate<T>(Expression expr)
            where T : Expression
        {
            Expression target = expr;

            while (!target.IsNil)
            {
                if (target.IsAtom)
                {
                    throw new ExpectedTypeException<Pair>(target);
                }

                yield return target.Car.Expect<T>();
                target = target.Cdr;
            }

            yield break;
        }

        public IEnumerator<Expression> GetEnumerator() => Enumerate(this).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Enumerate(this).GetEnumerator();

        #endregion
    }

    internal class SyntaxPair : Pair
    {
        private readonly HashSet<int> _marks;
        private readonly List<Tuple<Identifier, Symbol>> _subs;

        public override Expression Car => throw new UncategorizedException("Can't access Car of unexposed SyntaxPair.");
        public override Expression Cdr => throw new UncategorizedException("Can't access Cdr of unexposed SyntaxPair.");

        private SyntaxPair(Expression car, Expression cdr) : base(car, cdr)
        {
            _marks = new HashSet<int>();
            _subs = new List<Tuple<Identifier, Symbol>>();
        }

        public static Pair Wrap(Pair p)
        {
            return p switch
            {
                SyntaxPair sp => sp,
                _ => new SyntaxPair(p.Car, p.Cdr)
            };
        }

        public override Expression Mark(params int[] marks)
        {
            _marks.SymmetricExceptWith(marks);
            return this;
        }

        public override HashSet<int> GetMarks() => _marks;

        public override Expression Subst(Identifier id, Symbol sym)
        {
            _subs.Add(new Tuple<Identifier, Symbol>(id, sym));
            return this;
        }

        public override Expression Strip() => ConsIfNew(_car.Strip(), _cdr.Strip());
        public override Expression Expose()
        {
            return Cons(PushDown(this, _car), PushDown(this, _cdr));
        }

        private static Expression PushDown(SyntaxPair sp, Expression e)
        {
            Expression output = e.Mark(sp._marks.ToArray());

            foreach (var sub in sp._subs)
            {
                output.Subst(sub.Item1, sub.Item2);
            }

            return output;
        }
    }
}
