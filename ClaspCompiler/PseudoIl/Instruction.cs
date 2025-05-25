namespace ClaspCompiler.PseudoIl
{
    internal class Instruction : IInstruction
    {
        public Label? LineLabel { get; init; }
        public PseudoOp Operator { get; init; }
        public Instruction(PseudoOp op, Label? label = null)
        {
            LineLabel = label;
            Operator = op;
        }
        public override string ToString() => string.Format("({0}{1})",
            LineLabel is null ? string.Empty : $"{LineLabel}: ",
            Operator);
        public void Print(TextWriter writer, int indent) => writer.Write(ToString());
    }

    internal sealed class Instruction<T> : Instruction, IInstruction
        where T : IArgument
    {
        public T Argument { get; init; }
        public Instruction(PseudoOp op, T arg, Label? label = null)
            : base(op, label)
        {
            Argument = arg;
        }
        public override string ToString() => string.Format("({0}{1} {2})",
            LineLabel is null ? string.Empty : $"{LineLabel}: ",
            Operator,
            Argument);
    }
}
