using ClaspCompiler.IntermediateCil.Abstract;

namespace ClaspCompiler.IntermediateCil
{
    internal sealed class Instruction : IPrintable
    {
        public CilOp Operator { get; init; }
        public ICilArg? Operand { get; init; }

        public Instruction(CilOp op, ICilArg? arg = null)
        {
            Operator = op;
            Operand = arg;
        }

        public bool BreaksLine => false;
        public string AsString => $"({Operator}{(Operand is null ? string.Empty : ' ')}{Operand?.ToString() ?? string.Empty})";
        public void Print(TextWriter writer, int indent) => writer.Write(AsString);
        public override string ToString() => AsString;
    }
}
