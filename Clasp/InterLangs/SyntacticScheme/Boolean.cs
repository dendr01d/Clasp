using Clasp.AST;

namespace Clasp.InterLangs.SyntacticScheme
{
    internal sealed class Boolean : Expr, ITerminal<Prog, bool>
    {
        public static Boolean True = new(true);
        public static Boolean False = new(false);

        public bool Value { get; }

        private Boolean(bool b) => Value = b;

        public override string ToString() => Value ? "#t" : "#f";
    }
}
