using Clasp.AST;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Val : Form, ITerminal<Form, Expr>
    {
        public Expr Value { get; }

        public Val(Expr value) : base()
        {
            Value = value;
        }

        public override string ToString() => $"(val {Value})";
    }
}
