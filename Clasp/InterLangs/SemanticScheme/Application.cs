using Clasp.AST;
using Clasp.InterLangs.SyntacticScheme;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Application : Form, INonTerminal<Form>
    {
        public readonly Symbol Operator;
        public readonly Form[] Arguments;

        public Application(Symbol op, Form[] args)
        {
            Operator = op;
            Arguments = args;
        }

        public override string ToString()
        {
            return string.Format("(apply {0}{1}{2}{3}{4}",
                Operator,
                Arguments.Length > 0 ? " " : string.Empty,
                string.Join<Form>(' ', Arguments));
        }
    }
}
