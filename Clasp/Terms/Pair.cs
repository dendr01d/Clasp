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
        public override void SetCar(Expression expr) => _car = expr;
        public override void SetCdr(Expression expr) => _cdr = expr;


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

        public static Expression Append(Expression ls, Expression t)
        {
            return ls.IsNil
                ? t
                : Cons(ls.Car, Append(ls.Cdr, t));
        }

        public static Expression AppendLast(Expression ls, Expression t)
        {
            return ls.IsNil
                ? Cons(t, Nil)
                : Cons(ls.Car, AppendLast(ls.Cdr, t));
        }

        public static Boolean Member(Expression ls, Expression e)
        {
            return ls.IsNil
                ? Boolean.False
                : Pred_Equal(ls.Car, e)
                    ? Boolean.True
                    : Member(ls.Cdr, e);
        }

        #endregion

        public override string ToString() => $"({Car}{FormatContents(Cdr)})";

        private static string FormatContents(Expression expr)
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
                return $" {expr.Car}{FormatContents(expr.Cdr)}";
            }
        }
    }

    internal class Quoted : Pair
    {
        public Quoted(Expression exp) : base(Symbol.Quote, Cons(exp, Nil)) { }
    }

    internal class Quasiquoted : Pair
    {
        public Quasiquoted(Expression exp) : base(Symbol.Quasiquote, Cons(exp, Nil)) { }
    }

    internal class Unquoted : Pair
    {
        public Unquoted(Expression exp) : base(Symbol.Unquote, Cons(exp, Nil)) { }
    }

    internal class UnquotedSpliced : Pair
    {
        public UnquotedSpliced(Expression exp) : base(Symbol.UnquoteSplicing, Cons(exp, Nil)) { }
    }

    internal class EllipticPattern : Pair
    {
        public EllipticPattern(Expression exp) : base(exp, Cons(Symbol.Ellipsis, Nil)) { }
    }
}
