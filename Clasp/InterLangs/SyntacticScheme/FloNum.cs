using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class FloNum : Expr, ITerminal<Prog, double>
    {
        public double Value { get; }

        public FloNum(double value) : base()
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}
