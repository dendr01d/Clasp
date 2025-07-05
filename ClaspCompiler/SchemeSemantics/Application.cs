using ClaspCompiler.CompilerData;
using ClaspCompiler.SchemeSemantics.Abstract;

namespace ClaspCompiler.SchemeSemantics
{
    internal sealed record Application(ISemExp Operator, ISemExp[] Operands, uint AstId) : ISemExp
    {
        public bool BreaksLine => Operands.Any(x => x.BreaksLine);
        public string AsString => $"({Operator}{string.Concat(Operands.Select(x => $" {x}"))})";
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator, Operands, indent);
        public sealed override string ToString() => AsString;
    }
}
