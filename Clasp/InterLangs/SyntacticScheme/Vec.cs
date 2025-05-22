using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal class Vec : Expr, INonTerminal<Prog>
    {
        public readonly Expr[] Elements;

        public Vec(params Expr[] elements) : base()
        {
            Elements = elements;
        }

        public override string ToString()
        {
            return "#(" + string.Join<Expr>(' ', Elements) + ")";
        }
    }
}
