using ClaspCompiler.SchemeCore.Abstract;
using ClaspCompiler.SchemeTypes;

namespace ClaspCompiler.SchemeCore
{
    internal sealed record Application(ICoreExp Operator, ICoreExp[] Operands, SchemeType Type) : ICoreExp
    {
        public bool BreaksLine => Operands.Any(x => x.BreaksLine);
        public string AsString => Operator.StringifyApplication(Operands);
        public void Print(TextWriter writer, int indent) => writer.WriteApplication(Operator, Operands, indent);
        public sealed override string ToString() => AsString;
    }
}
