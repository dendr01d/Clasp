using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Cons : Expr, INonTerminal<Prog>
    {
        public Expr Car { get; private set; }
        public Expr Cdr { get; private set; }

        public Cons(Expr car, Expr cdr) : base()
        {
            Car = car;
            Cdr = cdr;
        }

        public override string ToString() => $"({Car}{TailToString(Cdr)})";

        private static string TailToString(Expr e)
        {
            return e switch
            {
                Nil => string.Empty,
                Cons c => c.Car.ToString() + TailToString(c.Cdr),
                _ => $" . {e}"
            };
        }
    }
}
