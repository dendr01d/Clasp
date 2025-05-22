using Clasp.AST;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Quote : Form, ITerminal<Form, Expr>
    {
        public Expr Value { get; }

        public Quote(Expr value) : base()
        {
            Value = value;
        }

        public override string ToString() => $"(quote {Value})";
    }
}
