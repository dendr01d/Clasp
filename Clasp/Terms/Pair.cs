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

        //public static Expression AppendLast(Expression ls, Expression t)
        //{
        //    return ls.IsNil
        //        ? Cons(t, Nil)
        //        : Cons(ls.Car, AppendLast(ls.Cdr, t));
        //}

        //public static Boolean Member(Expression ls, Expression e)
        //{
        //    return ls.IsNil
        //        ? Boolean.False
        //        : Pred_Equal(ls.Car, e)
        //            ? Boolean.True
        //            : Member(ls.Cdr, e);
        //}

        #endregion

        #region Helper Functions

        //left fold
        //not necessary unto itself, but helpful for certain arithmetic operators
        public static Expression Fold(Func<Expression, Expression, Expression> op, Expression init, Expression ls)
        {
            return ls.IsNil
                ? init
                : Fold(op, op(init, ls.Car), ls.Cdr);
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

        public override string ToPrinted() => $"({Car}{FormatContents(Cdr)})";
        public override string ToSerialized() => ToPrinted();

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

    //internal abstract class TaggedPair : Pair
    //{
    //    public readonly Expression TaggedValue;

    //    protected TaggedPair(Expression car, Expression cdr, Expression taggedValue) : base(car, cdr)
    //    {
    //        TaggedValue = taggedValue;
    //    }
    //}

    //internal class Quoted : TaggedPair
    //{
    //    public Quoted(Expression exp) : base(Symbol.Quote, Cons(exp, Nil), exp) { }
    //}

    //internal class Quasiquoted : TaggedPair
    //{
    //    public Quasiquoted(Expression exp) : base(Symbol.Quasiquote, Cons(exp, Nil), exp) { }
    //}

    //internal class Unquoted : TaggedPair
    //{
    //    public Unquoted(Expression exp) : base(Symbol.Unquote, Cons(exp, Nil), exp) { }
    //}

    //internal class UnquoteSpliced : TaggedPair
    //{
    //    public UnquoteSpliced(Expression exp) : base(Symbol.UnquoteSplicing, Cons(exp, Nil), exp) { }
    //}

    //internal class EllipticPattern : TaggedPair
    //{
    //    public EllipticPattern(Expression exp) : base(exp, Cons(Symbol.Ellipsis, Nil), exp) { }
    //}
}
