namespace ClaspCompiler.PseudoIl
{
    internal interface IInstruction : IPrintable
    {
        Label? LineLabel { get; }
        PseudoOp Operator { get; }
        IArgument? Operand { get; }
    }
}
