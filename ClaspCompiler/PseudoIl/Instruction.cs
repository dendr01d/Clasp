namespace ClaspCompiler.PseudoIl
{
    internal class Instruction : IInstruction
    {
        public Label? LineLabel { get; init; }
        public PseudoOp Operator { get; init; }
        public virtual IArgument? Operand { get; init; }
        public Instruction(PseudoOp op, IArgument? operand = null, Label? label = null)
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
