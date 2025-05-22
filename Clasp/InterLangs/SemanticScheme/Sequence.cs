using Clasp.AST;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Sequence : Form, INonTerminal<Form>
    {
        public readonly Form[] Sequents;

        public Sequence(params Form[] sequents) : base()
        {
            Sequents = sequents;
        }

        public override string ToString() => string.Format("(begin {0})", string.Join<Form>(' ', Sequents));
    }
}
