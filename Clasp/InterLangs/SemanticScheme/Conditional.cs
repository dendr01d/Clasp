using Clasp.AST;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Conditional : Form, INonTerminal<Form>
    {
        public readonly Form Condition;
        public readonly Form Consequent;
        public readonly Form Alternative;

        public Conditional(Form cond, Form consequent, Form alternative) : base()
        {
            Condition = cond;
            Consequent = consequent;
            Alternative = alternative;
        }

        public override string ToString() => string.Format("(if {0} {1} {2})",
            Condition,
            Consequent,
            Alternative);
    }
}
