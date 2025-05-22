using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Void : Expr, ITerminal<Prog, Void>
    {
        private static readonly Void _instance = new();

        public Void Value => _instance;

        private Void() { }

        public override string ToString() => "#void";
    }
}
