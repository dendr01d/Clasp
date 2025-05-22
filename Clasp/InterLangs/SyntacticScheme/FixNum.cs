using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class FixNum : Expr, ITerminal<Prog, int>
    {
        public int Value { get; }

        public FixNum(int value) : base()
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}
