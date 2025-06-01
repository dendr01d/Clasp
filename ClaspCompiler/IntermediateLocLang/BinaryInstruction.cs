using ClaspCompiler.CompilerData;
using ClaspCompiler.IntermediateLocLang.Abstract;

namespace ClaspCompiler.IntermediateLocLang
{
    internal class BinaryInstruction : IPrintable
    {
        public LocOp Operator { get; init; }
        public ILocArg? Argument { get; init; }
        public Var? Destination { get; init; }

        public BinaryInstruction(LocOp op, ILocArg? arg, Var? dest)
        {
            Operator = op;
            Argument = arg;
            Destination = dest;
        }

        public bool CanBreak => false;
        public override string ToString() => $"({Operator} {Argument} {Destination})";
        public void Print(TextWriter writer, int indent)
            => writer.WriteApplication(Operator.ToString(), [Argument, Destination ?? Var.NullVar], indent);
    }
}
