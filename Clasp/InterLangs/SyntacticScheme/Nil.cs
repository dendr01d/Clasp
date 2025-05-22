using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Nil : Expr, ITerminal<Prog, Nil>
    {
        private static readonly Nil _instance = new();

        public Nil Value => _instance;

        private Nil() { }

        public override string ToString() => "()";
    }
}
