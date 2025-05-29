using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Let : ISpecialForm
    {
        public readonly Var Variable;
        public readonly ISemanticExp Argument;
        public readonly ISemanticExp Body;

        public Let(Var var, ISemanticExp arg, ISemanticExp body)
        {
            Variable = var;
            Argument = arg;
            Body = body;
        }

        public bool CanBreak => true;
        public override string ToString()
        {
            return string.Format("(let ([{0} {1}]) {2})", Variable, Argument, Body);
        }

        public void Print(TextWriter writer, int indent)
        {
            writer.WriteIndenting("(let ", ref indent);
            int restIndent = indent;

            writer.WriteIndenting("([", ref indent);
            writer.WriteIndenting(Variable, ref indent);
            writer.WriteIndenting(' ', ref indent);

            writer.Write(Argument, indent);

            writer.WriteLineIndent("])", restIndent);
            writer.Write(Body, restIndent);

            writer.Write(')');
        }
    }
}
