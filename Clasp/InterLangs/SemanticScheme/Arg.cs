using Clasp.AST;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Arg : Form, ITerminal<Form, Symbol>
    {
        public Symbol Value { get; }

        public Arg(Symbol value) : base()
        {
            Value = value;
        }

        public override string ToString() => Value.ToString();
    }
}
