using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Symbol : Expr, ITerminal<Prog, string>
    {
        public string Value { get; }

        public Symbol(string value) : base()
        {
            Value = value;
        }

        public override string ToString() => Value;
    }
}
