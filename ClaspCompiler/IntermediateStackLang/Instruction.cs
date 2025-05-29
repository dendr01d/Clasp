using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal class Instruction : IStackInstr
    {
        public Label? LineLabel { get; init; }
        public StackOp Operator { get; init; }
        public virtual IStackArg? Operand { get; init; }
        public Instruction(StackOp op, IStackArg? operand = null, Label? label = null)
        {
            LineLabel = label;
            Operator = op;
            Operand = operand;
        }
        public override string ToString() => string.Format("({0}{1}{2})",
            LineLabel is null ? string.Empty : $"{LineLabel}: ",
            Operator,
            Operand is null ? string.Empty : $" {Operand}");
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
