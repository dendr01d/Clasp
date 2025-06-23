using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Apply(ISemExp Operator, ISemExp[] Operands) : ISemExp
    {
        public bool BreaksLine => Operands.Any(x => x.BreaksLine);
        public string AsString => Operator.StringifyApplication(Operands);
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator, Operands, indent);
        public sealed override string ToString() => AsString;
    }
}
