using Clasp.AST;

namespace Clasp.InterLangs.SemanticScheme
{
    internal sealed class Lambda : Form, INonTerminal<Form>
    {
        public readonly Arg[] Parameters;
        public readonly Arg? VariadicParameter;
        public readonly Sequence Body;

        public Lambda(Arg[] parms, Arg? variad, Sequence body) : base()
        {
            Parameters = parms;
            VariadicParameter = variad;
            Body = body;
        }

        public override string ToString()
        {
            string paramList = string.Format("{0}{1}{2}{3}{4}",
                Parameters.Length > 0 || VariadicParameter is null ? "(" : string.Empty,
                Parameters.Length > 0 ? string.Join<Arg>(' ', Parameters) : string.Empty,
                Parameters.Length > 0 && VariadicParameter is not null ? " . " : string.Empty,
                VariadicParameter is not null ? VariadicParameter.ToString() : string.Empty,
                Parameters.Length > 0 || VariadicParameter is null ? ")" : string.Empty);

            return string.Format("(lambda {0} {1})",
                paramList,
                string.Join<Form>(' ', Body.Sequents));
        }
    }
}
