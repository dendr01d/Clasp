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
        public override void SetCar(Expression expr) => _car = expr;
        public override void SetCdr(Expression expr) => _cdr = expr;


        #region Constructors

        public static Pair Cons(Expression car, Expression cdr) => new Pair(car, cdr);

        public static Expression List(params Expression[] terms)
        {
            if (terms.Length == 0)
            {
                return Nil;
            }
            else
            {
                return Cons(terms[0], List(terms[1..]));
            }
        }

        public static Pair ListStar(params Expression[] terms)
        {
            if (terms.Length < 2)
            {
                throw new MissingArgumentException("C# List*");
            }
            else if (terms.Length == 2)
            {
                return Cons(terms[0], terms[1]);
            }
            else
            {
                return Cons(terms[0], ListStar(terms[1..]));
            }
        }

        public static Expression Append(Expression ls, Expression t)
        {
            return ls.IsNil
                ? t
                : Cons(ls.Car, Append(ls.Cdr, t));
        }

        public static Expression FoldL(Expression ls, Expression seed, Func<Expression, Expression, Expression> op)
        {
            while (!ls.IsNil)
            {
                seed = op(seed, ls.Car);
                ls = ls.Cdr;
            }
            return seed;
        }

        public static Expression FoldR(Expression ls, Expression seed, Func<Expression, Expression, Expression> op)
        {
            if (ls.IsNil)
            {
                return seed;
            }
            else
            {
                Expression sub = FoldR(ls.Cdr, seed, op);
                return op(ls.Car, sub);
            }
        }

        public static Expression MapCar(Expression ls, Func<Expression, Expression> map)
        {
            if (ls.IsNil)
            {
                return ls;
            }
            else
            {
                return Cons(map(ls.Car), MapCar(ls.Cdr, map));
            }
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
}
