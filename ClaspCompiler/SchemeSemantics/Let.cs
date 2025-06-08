using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed class Let : ISemSpec
    {
        public SpecialKeyword Keyword => SpecialKeyword.Let;

        public readonly Var Variable;
        public readonly ISemExp Argument;
        public readonly ISemExp Body;

        public Let(Var var, ISemExp arg, ISemExp body)
        {
            Variable = var;
            Argument = arg;
            Body = body;
        }

        public bool BreaksLine => true;
        public string AsString => string.Format("(let ([{0} {1}]) {2})", Variable, Argument, Body);
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
        public override string ToString() => AsString;
    }
}
