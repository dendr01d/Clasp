using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Str : Expr, ITerminal<Prog, string>
    {
        public string Value { get; }

        public Str(string value) : base()
        {
            Value = value;
        }

        public override string ToString() => $"\"{Value}\"";
    }
}
