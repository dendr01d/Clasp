using ClaspCompiler.IntermediateStackLang.Abstract;

namespace ClaspCompiler.IntermediateStackLang
{
    internal class UnaryInstruction : IPrintable
    {
        public StackOp Operator { get; init; }
        public virtual IStackArg? Operand { get; init; }
        public UnaryInstruction(StackOp op, IStackArg? operand = null)
        {
            Operator = op;
            Operand = operand;
        }

        public bool CanBreak => true;
        public override string ToString() => string.Format("({0}{1})",
            Operator,
            Operand is null ? string.Empty : $" {Operand}");
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }
}
