using Clasp.AST;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Var : Form, ITerminal<Form, Symbol>
    {
        public Symbol Value { get; }

        public Var(Symbol value) : base()
        {
            Value = value;
        }

        public override string ToString() => $"(var {Value})";
    }
}
